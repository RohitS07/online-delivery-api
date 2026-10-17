using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OnlineDelivery.Api.Data;
using OnlineDelivery.Api.Entities;
using OnlineDelivery.Api.Tests.Support;

namespace OnlineDelivery.Api.Tests;

[Collection(ApiCollection.Name)]
public class ReferenceDataTests(ApiFactory factory)
{
    // AC-0004-01
    [Fact]
    public async Task SchemaAndSeedComeFromMigrations()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.NotEmpty(db.Database.GetMigrations());
        Assert.Equal(db.Database.GetMigrations(), await db.Database.GetAppliedMigrationsAsync());
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.Equal(2, await db.Cities.CountAsync());
        Assert.Equal(4, await db.Villages.CountAsync());
        Assert.Equal(6, await db.Categories.CountAsync());
    }

    // AC-0004-02
    [Fact]
    public async Task CitiesReturnSeedWithVillages()
    {
        var cities = await GetJson("/api/v1/cities");

        Assert.Equal(["Mumbai", "Pune"], cities.EnumerateArray().Select(c => c.GetProperty("nameEn").GetString()));
        var mumbai = cities[0];
        Assert.Equal(["id", "nameEn", "nameMr", "villages"], mumbai.EnumerateObject().Select(p => p.Name));
        Assert.Equal("मुंबई", mumbai.GetProperty("nameMr").GetString());
        Assert.Equal(["Matheran", "Pali"], VillageNames(mumbai));
        Assert.Equal(["Jejuri", "Mulshi"], VillageNames(cities[1]));

        var matheran = mumbai.GetProperty("villages")[0];
        Assert.Equal(["id", "nameEn", "nameMr", "deliveryHours"], matheran.EnumerateObject().Select(p => p.Name));
        Assert.Equal("माथेरान", matheran.GetProperty("nameMr").GetString());
        foreach (var city in cities.EnumerateArray())
        {
            Assert.All(city.GetProperty("villages").EnumerateArray(), v => Assert.Equal(3, v.GetProperty("deliveryHours").GetInt32()));
        }
    }

    // AC-0004-03
    [Fact]
    public async Task InactiveVillageIsHidden()
    {
        await SetActive<Village>(v => v.NameEn == "Pali", false);
        try
        {
            var cities = await GetJson("/api/v1/cities");

            var mumbai = cities.EnumerateArray().Single(c => c.GetProperty("nameEn").GetString() == "Mumbai");
            Assert.Equal(["Matheran"], VillageNames(mumbai));
        }
        finally
        {
            await SetActive<Village>(v => v.NameEn == "Pali", true);
        }
    }

    // AC-0004-04
    [Fact]
    public async Task InactiveCityHidesItsVillages()
    {
        await SetActive<City>(c => c.NameEn == "Pune", false);
        try
        {
            var cities = await GetJson("/api/v1/cities");

            var only = Assert.Single(cities.EnumerateArray());
            Assert.Equal("Mumbai", only.GetProperty("nameEn").GetString());
            Assert.DoesNotContain("Mulshi", cities.GetRawText());
            Assert.DoesNotContain("Jejuri", cities.GetRawText());
        }
        finally
        {
            await SetActive<City>(c => c.NameEn == "Pune", true);
        }
    }

    // AC-0004-05
    [Fact]
    public async Task CategoriesReturnSeedSorted()
    {
        var categories = await GetJson("/api/v1/categories");

        Assert.Equal(
            ["Dairy", "Fruits", "Groceries", "Household", "Personal care", "Vegetables"],
            categories.EnumerateArray().Select(c => c.GetProperty("nameEn").GetString()));
        Assert.Equal(["id", "nameEn", "nameMr"], categories[0].EnumerateObject().Select(p => p.Name));
        Assert.Equal("दुग्धजन्य पदार्थ", categories[0].GetProperty("nameMr").GetString());
    }

    // AC-0004-06
    [Fact]
    public async Task CityNameIsUnique()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Cities.Add(new City { NameEn = "Pune", NameMr = "पुणे", IsActive = true });

        var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());

        var postgres = Assert.IsType<PostgresException>(error.InnerException);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, postgres.SqlState);
        Assert.Equal("IX_Cities_NameEn", postgres.ConstraintName);
    }

    private async Task<JsonElement> GetJson(string path)
    {
        var response = await factory.CreateClient().GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    }

    private static IEnumerable<string?> VillageNames(JsonElement city) =>
        city.GetProperty("villages").EnumerateArray().Select(v => v.GetProperty("nameEn").GetString());

    private async Task SetActive<T>(System.Linq.Expressions.Expression<Func<T, bool>> which, bool active)
        where T : class, IActivatable
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Set<T>().Where(which).ExecuteUpdateAsync(s => s.SetProperty(e => e.IsActive, active));
    }
}
