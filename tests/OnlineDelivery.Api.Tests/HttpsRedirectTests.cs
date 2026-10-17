using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using OnlineDelivery.Api.Tests.Support;

namespace OnlineDelivery.Api.Tests;

[Collection(ApiCollection.Name)]
public class HttpsRedirectTests(ApiFactory factory)
{
    private static readonly WebApplicationFactoryClientOptions NoRedirects = new() { AllowAutoRedirect = false };

    // AC-0001-06
    [Fact]
    public async Task ProductionRedirectsHttpToHttps()
    {
        var client = factory
            .WithWebHostBuilder(b => b.UseEnvironment("Production").UseSetting("https_port", "443"))
            .CreateClient(NoRedirects);

        var response = await client.GetAsync("/api/v1/health");

        Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
        Assert.Equal("https://localhost/api/v1/health", response.Headers.Location?.ToString());
    }

    // AC-0001-07
    [Fact]
    public async Task DevelopmentServesHttp()
    {
        var client = factory
            .WithWebHostBuilder(b => b.UseEnvironment("Development").UseSetting("https_port", "443"))
            .CreateClient(NoRedirects);

        var response = await client.GetAsync("/api/v1/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
