using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OnlineDelivery.Api.Data;
using OnlineDelivery.Api.Entities;

namespace OnlineDelivery.Api.Tests.Support;

/// <summary>Helpers for the sign-in tests of US-0005.</summary>
public static class AuthSupport
{
    private static long _next = 9_800_000_000;

    /// <summary>A mobile number no other test uses, as the API stores it.</summary>
    public static string NewMobileNumber() => "+91" + Interlocked.Increment(ref _next);

    public static Task<HttpResponseMessage> RequestCode(this HttpClient client, string mobileNumber) =>
        client.PostAsJsonAsync("/api/v1/auth/otp", new { mobileNumber });

    public static Task<HttpResponseMessage> Verify(this HttpClient client, string mobileNumber, string code) =>
        client.PostAsJsonAsync("/api/v1/auth/otp/verify", new { mobileNumber, code });

    public static async Task<JsonElement> Json(this HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

    public static async Task<string?> ProblemCode(this HttpResponseMessage response) =>
        (await response.Json()).TryGetProperty("code", out var code) ? code.GetString() : null;

    public static async Task<User> AddUser(this ApiFactory factory, UserRole role, UserStatus status = UserStatus.Active)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            MobileNumber = NewMobileNumber(),
            Name = $"Test {role}",
            Role = role,
            Status = status,
            CreatedAt = factory.Clock.GetUtcNow(),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public static async Task SetStatus(this ApiFactory factory, Guid userId, UserStatus status)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FindAsync(userId);
        user!.Status = status;
        await db.SaveChangesAsync();
    }

    /// <summary>Requests and verifies a code, and returns the verify answer.</summary>
    public static async Task<JsonElement> SignIn(this ApiFactory factory, HttpClient client, string mobileNumber)
    {
        await client.RequestCode(mobileNumber);
        var response = await client.Verify(mobileNumber, factory.Otp.LastCodeFor(mobileNumber));
        return await response.Json();
    }

    public static HttpRequestMessage WithBearer(this HttpRequestMessage request, string token)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
