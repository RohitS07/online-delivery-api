using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace OnlineDelivery.Api.Tests.Support;

/// <summary>The API in memory, on a real PostgreSQL in Docker: ADR-0004.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16").Build();

    public Task InitializeAsync() => _postgres.StartAsync();

    // Development, so the API migrates this database on start-up: ADR-0004.
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseEnvironment("Development")
            .UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());

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
