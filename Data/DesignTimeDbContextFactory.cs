using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OnlineDelivery.Api.Data;

/// <summary>Lets `dotnet ef` build migrations without a database or a secret: ADR-0004.</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql("Host=localhost;Database=design_time").Options);
}
