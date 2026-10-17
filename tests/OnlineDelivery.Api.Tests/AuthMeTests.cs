using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineDelivery.Api.Data;
using OnlineDelivery.Api.Entities;
using OnlineDelivery.Api.Tests.Support;

namespace OnlineDelivery.Api.Tests;

[Collection(ApiCollection.Name)]
public class AuthMeTests(ApiFactory factory)
{
    private const string Me = "/api/v1/auth/me";

    // AC-0005-12
    [Fact]
    public async Task MeAcceptsOnlyAccessTokens()
    {
        var client = factory.CreateClient();
        var agent = await factory.AddUser(UserRole.Agent);
        var accessToken = (await factory.SignIn(client, agent.MobileNumber)).GetProperty("accessToken").GetString()!;
        var registrationToken = (await factory.SignIn(client, AuthSupport.NewMobileNumber()))
            .GetProperty("registrationToken").GetString()!;

        var ok = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, Me).WithBearer(accessToken));
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        var user = await ok.Json();
        Assert.Equal(agent.Id, user.GetProperty("id").GetGuid());
        Assert.Equal("Agent", user.GetProperty("role").GetString());

        var registration = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, Me).WithBearer(registrationToken));
        Assert.Equal(HttpStatusCode.Unauthorized, registration.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(Me)).StatusCode);
    }

    // AC-0005-13
    [Fact]
    public async Task BlockedUserTokenStopsWorking()
    {
        var client = factory.CreateClient();
        var customer = await factory.AddUser(UserRole.Customer);
        var token = (await factory.SignIn(client, customer.MobileNumber)).GetProperty("accessToken").GetString()!;

        await factory.SetStatus(customer.Id, UserStatus.Blocked);
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, Me).WithBearer(token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("user.blocked", await response.ProblemCode());
    }

    // AC-0005-13 also covers a role that changed after the token was issued.
    [Fact]
    public async Task ChangedRoleTokenStopsWorking()
    {
        var client = factory.CreateClient();
        var customer = await factory.AddUser(UserRole.Customer);
        var token = (await factory.SignIn(client, customer.MobileNumber)).GetProperty("accessToken").GetString()!;

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Users.Where(u => u.Id == customer.Id).ExecuteUpdateAsync(s => s.SetProperty(u => u.Role, UserRole.Agent));
        }

        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, Me).WithBearer(token));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // AC-0005-14: the test database starts empty, so no admin existed before start-up.
    [Fact]
    public async Task FirstAdminComesFromConfiguration()
    {
        var configured = factory.Services.GetRequiredService<IConfiguration>()["Auth:FirstAdmin:MobileNumber"];
        Assert.False(string.IsNullOrEmpty(configured));

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var admins = await db.Users.Where(u => u.Role == UserRole.Admin).ToListAsync();

        var admin = Assert.Single(admins);
        Assert.Equal(configured, admin.MobileNumber);
        Assert.Equal(UserStatus.Active, admin.Status);
    }
}
