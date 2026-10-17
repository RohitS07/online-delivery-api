using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using OnlineDelivery.Api.Services.Auth;
using Testcontainers.PostgreSql;

namespace OnlineDelivery.Api.Tests.Support;

/// <summary>The API in memory, on a real PostgreSQL in Docker: ADR-0004.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // An obvious fake that could not sign anything real: RULE-0014.
    public const string FakeSigningKey = "fake-test-signing-key-not-a-secret-0123456789";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16").Build();

    /// <summary>The API's clock, which tests move forward: ADR-0007.</summary>
    public FakeTimeProvider Clock { get; } = new(TimeProvider.System.GetUtcNow());

    /// <summary>Every code the API sent, instead of an SMS: ADR-0009.</summary>
    public RecordingOtpSender Otp { get; } = new();

    public Task InitializeAsync() => _postgres.StartAsync();

    // Development, so the API migrates this database on start-up: ADR-0004.
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseEnvironment("Development")
            .UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString())
            .UseSetting("Jwt:SigningKey", FakeSigningKey)
            .ConfigureTestServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(Clock);
                services.RemoveAll<IOtpSender>();
                services.AddSingleton<IOtpSender>(Otp);
            });

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}

/// <summary>Every test class shares one database, and runs one at a time.</summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "Api";
}
