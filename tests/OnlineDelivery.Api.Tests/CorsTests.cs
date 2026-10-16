using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using OnlineDelivery.Api.Tests.Support;

namespace OnlineDelivery.Api.Tests;

public class CorsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private const string AppOrigin = "http://localhost:5173";
    private const string OtherOrigin = "https://unlisted.example";

    // AC-0001-02: Development's appsettings lists the app's origin.
    [Fact]
    public async Task AllowedOriginGetsCorsHeader()
    {
        var response = await SendFrom(factory.CreateClient(), AppOrigin);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(AppOrigin, Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
    }

    // AC-0001-03
    [Fact]
    public async Task UnlistedOriginGetsNoCorsHeader()
    {
        var response = await SendFrom(factory.CreateClient(), OtherOrigin);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    // AC-0001-04: CORS answers the preflight before authorization can refuse it.
    [Fact]
    public async Task PreflightToProtectedEndpointSucceeds()
    {
        var client = factory.WithWebHostBuilder(b => b.ConfigureTestServices(ProtectedProbe.Register)).CreateClient();
        var preflight = new HttpRequestMessage(HttpMethod.Options, ProtectedProbe.Route);
        preflight.Headers.Add("Origin", AppOrigin);
        preflight.Headers.Add("Access-Control-Request-Method", "GET");
        preflight.Headers.Add("Access-Control-Request-Headers", "authorization");

        var response = await client.SendAsync(preflight);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(AppOrigin, Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
        var unauthenticated = await client.GetAsync(ProtectedProbe.Route);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthenticated.StatusCode);
    }

    // AC-0001-05: Production's appsettings lists no origin.
    [Fact]
    public async Task EmptyListAllowsNoOrigin()
    {
        var client = factory.WithWebHostBuilder(b => b.UseEnvironment("Production"))
            .CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        foreach (var origin in new[] { AppOrigin, OtherOrigin })
        {
            var response = await SendFrom(client, origin);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        }
    }

    private static Task<HttpResponseMessage> SendFrom(HttpClient client, string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/health");
        request.Headers.Add("Origin", origin);
        return client.SendAsync(request);
    }
}
