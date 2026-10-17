using Microsoft.EntityFrameworkCore;
using OnlineDelivery.Api.Data;
using OnlineDelivery.Api.Entities;

namespace OnlineDelivery.Api.Services.Auth;

/// <summary>The first admin from configuration, and demo users in Development: SPEC-0003.</summary>
public static class AuthSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IHostEnvironment environment)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var seeds = new List<SeedUser>();
        var admin = configuration.GetSection("Auth:FirstAdmin").Get<SeedUser>();
        if (admin is { MobileNumber.Length: > 0 })
        {
            seeds.Add(admin with { Role = UserRole.Admin });
        }

        if (environment.IsDevelopment())
        {
            seeds.AddRange(configuration.GetSection("Auth:DemoUsers").Get<List<SeedUser>>() ?? []);
        }

        if (seeds.Count == 0)
        {
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow();
        foreach (var seed in seeds)
        {
            if (!MobileNumber.TryNormalize(seed.MobileNumber, out var number))
            {
                throw new InvalidOperationException($"Seed user '{seed.Name}' has an invalid mobile number.");
            }

            // A number already in use is left alone, whatever its role.
            var exists = await db.Users.AnyAsync(u => u.MobileNumber == number)
                || (seed.Role == UserRole.Admin && await db.Users.AnyAsync(u => u.Role == UserRole.Admin));
            if (!exists)
            {
                db.Users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    MobileNumber = number,
                    Name = seed.Name,
                    Role = seed.Role,
                    Status = UserStatus.Active,
                    CreatedAt = now,
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private sealed record SeedUser(string MobileNumber, string Name, UserRole Role = UserRole.Customer);
}
