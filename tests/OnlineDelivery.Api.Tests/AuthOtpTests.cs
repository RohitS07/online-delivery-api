using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineDelivery.Api.Data;
using OnlineDelivery.Api.Entities;
using OnlineDelivery.Api.Tests.Support;

namespace OnlineDelivery.Api.Tests;

[Collection(ApiCollection.Name)]
public class AuthOtpTests(ApiFactory factory)
{
    // AC-0005-01
    [Fact]
    public async Task RequestSendsCode()
    {
        var stored = AuthSupport.NewMobileNumber();
        var written = stored[3..8] + " " + stored[8..];

        var response = await factory.CreateClient().RequestCode(written);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var body = await response.Json();
        Assert.Equal(30, body.GetProperty("resendAfterSeconds").GetInt32());
        Assert.Equal(300, body.GetProperty("expiresInSeconds").GetInt32());
        var code = Assert.Single(factory.Otp.SentTo(stored));
        Assert.Matches("^[0-9]{6}$", code);
    }

    // AC-0005-02
    [Fact]
    public async Task InvalidNumberIsRefused()
    {
        var sentBefore = factory.Otp.TotalSent;

        var response = await factory.CreateClient().RequestCode("12345");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("mobile.invalid", await response.ProblemCode());
        Assert.Equal(sentBefore, factory.Otp.TotalSent);
    }

    // AC-0005-03
    [Fact]
    public async Task ResendTooSoonIsRefused()
    {
        var client = factory.CreateClient();
        var number = AuthSupport.NewMobileNumber();
        await client.RequestCode(number);
        factory.Clock.Advance(TimeSpan.FromSeconds(10));

        var response = await client.RequestCode(number);

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.Equal("otp.tooSoon", await response.ProblemCode());
        Assert.Single(factory.Otp.SentTo(number));
    }

    // AC-0005-04
    [Fact]
    public async Task SixthRequestInAnHourIsRefused()
    {
        var client = factory.CreateClient();
        var number = AuthSupport.NewMobileNumber();
        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(HttpStatusCode.Accepted, (await client.RequestCode(number)).StatusCode);
            factory.Clock.Advance(TimeSpan.FromSeconds(31));
        }

        var response = await client.RequestCode(number);

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.Equal("otp.tooManyRequests", await response.ProblemCode());
        Assert.Equal(5, factory.Otp.SentTo(number).Count);
    }

    // AC-0005-05
    [Fact]
    public async Task RequestDoesNotRevealAccount()
    {
        var client = factory.CreateClient();
        var known = await factory.AddUser(UserRole.Customer);
        var blocked = await factory.AddUser(UserRole.Customer, UserStatus.Blocked);
        var unknown = AuthSupport.NewMobileNumber();

        var answers = new List<(HttpStatusCode, string)>();
        foreach (var number in new[] { known.MobileNumber, blocked.MobileNumber, unknown })
        {
            var response = await client.RequestCode(number);
            answers.Add((response.StatusCode, await response.Content.ReadAsStringAsync()));
        }

        Assert.Equal(HttpStatusCode.Accepted, answers[0].Item1);
        Assert.All(answers, answer => Assert.Equal(answers[0], answer));
    }

    // AC-0005-03 and AC-0005-04 hold when requests arrive at the same moment.
    [Fact]
    public async Task ParallelRequestsSendOneCode()
    {
        var client = factory.CreateClient();
        var number = AuthSupport.NewMobileNumber();

        var responses = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => client.RequestCode(number)));

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Accepted);
        Assert.Single(factory.Otp.SentTo(number));
    }

    // AC-0005-06
    [Fact]
    public async Task CodeIsStoredHashed()
    {
        var number = AuthSupport.NewMobileNumber();
        await factory.CreateClient().RequestCode(number);
        var code = factory.Otp.LastCodeFor(number);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var challenge = await db.OtpChallenges.SingleAsync(c => c.MobileNumber == number);
        Assert.NotEmpty(challenge.CodeHash);
        Assert.DoesNotContain(code, challenge.CodeHash);
        Assert.NotEqual(code, challenge.CodeHash);
    }
}
