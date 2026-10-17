using System.Net;
using Microsoft.IdentityModel.JsonWebTokens;
using OnlineDelivery.Api.Entities;
using OnlineDelivery.Api.Tests.Support;

namespace OnlineDelivery.Api.Tests;

[Collection(ApiCollection.Name)]
public class AuthVerifyTests(ApiFactory factory)
{
    private static string WrongCode(string code) => code == "000000" ? "111111" : "000000";

    // AC-0005-07
    [Fact]
    public async Task KnownNumberGetsAccessToken()
    {
        var customer = await factory.AddUser(UserRole.Customer);

        var body = await factory.SignIn(factory.CreateClient(), customer.MobileNumber);

        Assert.Equal("signedIn", body.GetProperty("result").GetString());
        var user = body.GetProperty("user");
        Assert.Equal(customer.Id, user.GetProperty("id").GetGuid());
        Assert.Equal("Customer", user.GetProperty("role").GetString());
        Assert.Equal("Active", user.GetProperty("status").GetString());
        Assert.Equal(customer.Name, user.GetProperty("name").GetString());

        var token = new JsonWebTokenHandler().ReadJsonWebToken(body.GetProperty("accessToken").GetString());
        Assert.Equal(customer.Id.ToString(), token.Subject);
        Assert.Equal("Customer", token.GetClaim("role").Value);
        Assert.Equal(TimeSpan.FromDays(7), token.ValidTo - token.IssuedAt);
        Assert.Equal(token.ValidTo, body.GetProperty("expiresAt").GetDateTime().ToUniversalTime(), TimeSpan.FromSeconds(1));
    }

    // AC-0005-08
    [Fact]
    public async Task UnknownNumberGetsRegistrationToken()
    {
        var number = AuthSupport.NewMobileNumber();

        var body = await factory.SignIn(factory.CreateClient(), number);

        Assert.Equal("registrationRequired", body.GetProperty("result").GetString());
        Assert.Equal(System.Text.Json.JsonValueKind.Null, body.GetProperty("user").ValueKind);
        Assert.False(body.TryGetProperty("accessToken", out _));
        var token = new JsonWebTokenHandler().ReadJsonWebToken(body.GetProperty("registrationToken").GetString());
        Assert.Equal("registration", token.GetClaim("purpose").Value);
        Assert.Equal(number, token.GetClaim("mobile_number").Value);
        Assert.Equal(TimeSpan.FromMinutes(15), token.ValidTo - token.IssuedAt);
    }

    // AC-0005-09
    [Fact]
    public async Task FifthWrongCodeVoidsCode()
    {
        var client = factory.CreateClient();
        var number = AuthSupport.NewMobileNumber();
        await client.RequestCode(number);
        var code = factory.Otp.LastCodeFor(number);

        for (var i = 0; i < 4; i++)
        {
            var wrong = await client.Verify(number, WrongCode(code));
            Assert.Equal(HttpStatusCode.BadRequest, wrong.StatusCode);
            Assert.Equal("otp.invalid", await wrong.ProblemCode());
        }
        var fifth = await client.Verify(number, WrongCode(code));
        Assert.Equal(HttpStatusCode.TooManyRequests, fifth.StatusCode);
        Assert.Equal("otp.tooManyAttempts", await fifth.ProblemCode());

        var right = await client.Verify(number, code);
        Assert.Equal(HttpStatusCode.BadRequest, right.StatusCode);
        Assert.Equal("otp.expired", await right.ProblemCode());
    }

    // AC-0005-09 holds when guesses arrive at the same moment.
    [Fact]
    public async Task ParallelGuessesStopAtFive()
    {
        var client = factory.CreateClient();
        var number = AuthSupport.NewMobileNumber();
        await client.RequestCode(number);
        var code = factory.Otp.LastCodeFor(number);

        var responses = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => client.Verify(number, WrongCode(code))));
        var codes = await Task.WhenAll(responses.Select(r => r.ProblemCode()));

        Assert.Equal(4, codes.Count(c => c == "otp.invalid"));
        Assert.Equal(1, codes.Count(c => c == "otp.tooManyAttempts"));
        Assert.Equal(15, codes.Count(c => c == "otp.expired"));
        Assert.Equal("otp.expired", await (await client.Verify(number, code)).ProblemCode());
    }

    // AC-0005-10 holds when the same code arrives twice at the same moment.
    [Fact]
    public async Task ParallelRightCodesSignInOnce()
    {
        var client = factory.CreateClient();
        var customer = await factory.AddUser(UserRole.Customer);
        await client.RequestCode(customer.MobileNumber);
        var code = factory.Otp.LastCodeFor(customer.MobileNumber);

        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => client.Verify(customer.MobileNumber, code)));

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.OK);
    }

    // AC-0005-10
    [Fact]
    public async Task ExpiredOrUsedCodeIsRefused()
    {
        var client = factory.CreateClient();
        var late = AuthSupport.NewMobileNumber();
        await client.RequestCode(late);
        factory.Clock.Advance(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(1));

        var expired = await client.Verify(late, factory.Otp.LastCodeFor(late));
        Assert.Equal(HttpStatusCode.BadRequest, expired.StatusCode);
        Assert.Equal("otp.expired", await expired.ProblemCode());

        var reused = AuthSupport.NewMobileNumber();
        await client.RequestCode(reused);
        var code = factory.Otp.LastCodeFor(reused);
        Assert.Equal(HttpStatusCode.OK, (await client.Verify(reused, code)).StatusCode);
        var again = await client.Verify(reused, code);
        Assert.Equal(HttpStatusCode.BadRequest, again.StatusCode);
        Assert.Equal("otp.expired", await again.ProblemCode());
    }

    // AC-0005-11
    [Fact]
    public async Task BlockedUserGetsNoToken()
    {
        var client = factory.CreateClient();
        var blocked = await factory.AddUser(UserRole.Customer, UserStatus.Blocked);
        await client.RequestCode(blocked.MobileNumber);

        var response = await client.Verify(blocked.MobileNumber, factory.Otp.LastCodeFor(blocked.MobileNumber));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("user.blocked", await response.ProblemCode());
        var text = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("accessToken", text);
        Assert.DoesNotContain("registrationToken", text);
    }
}
