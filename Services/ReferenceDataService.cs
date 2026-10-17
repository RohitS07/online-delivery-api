using Microsoft.EntityFrameworkCore;
using OnlineDelivery.Api.Data;
using OnlineDelivery.Api.Dtos;

namespace OnlineDelivery.Api.Services;

/// <summary>Active cities, villages and categories, sorted by English name: SPEC-0002.</summary>
public class ReferenceDataService(AppDbContext db)
{
    public async Task<IReadOnlyList<CityResponse>> ListCitiesAsync(CancellationToken cancellationToken)
    {
        var cities = await db.Cities.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.NameEn)
            .Select(c => new
            {
                c.Id,
                c.NameEn,
                c.NameMr,
                Villages = c.Villages.Where(v => v.IsActive).OrderBy(v => v.NameEn)
                    .Select(v => new VillageResponse(v.Id, v.NameEn, v.NameMr, v.DeliveryHours)).ToList(),
            })
            .ToListAsync(cancellationToken);
        return cities.Select(c => new CityResponse(c.Id, c.NameEn, c.NameMr, c.Villages)).ToList();
    }

    public async Task<IReadOnlyList<CategoryResponse>> ListCategoriesAsync(CancellationToken cancellationToken) =>
        await db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.NameEn)
            .Select(c => new CategoryResponse(c.Id, c.NameEn, c.NameMr))
            .ToListAsync(cancellationToken);
}
