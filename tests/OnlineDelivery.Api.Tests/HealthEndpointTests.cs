using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace OnlineDelivery.Api.Tests;

public class HealthEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    // AC-0001-01: the body is the HealthResponse of SPEC-0001.
    [Fact]
    public async Task ReturnsSpecBody()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var properties = body.RootElement.EnumerateObject().Select(p => p.Name).ToArray();
        Assert.Equal(["status", "service", "timestamp"], properties);
        Assert.Equal("healthy", body.RootElement.GetProperty("status").GetString());
        Assert.Equal("online-delivery-api", body.RootElement.GetProperty("service").GetString());

        var timestamp = body.RootElement.GetProperty("timestamp").GetString()!;
        Assert.EndsWith("Z", timestamp);
        var parsed = DateTime.Parse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        Assert.InRange(parsed, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }
}
