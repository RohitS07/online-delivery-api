using Microsoft.EntityFrameworkCore;
using OnlineDelivery.Api.Entities;

namespace OnlineDelivery.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Seeded rows use ids below this, and identity starts above them: SPEC-0002.
    private const long FirstGeneratedId = 1000;

    public DbSet<City> Cities => Set<City>();
    public DbSet<Village> Villages => Set<Village>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<City>(city =>
        {
            city.Property(c => c.Id).HasIdentityOptions(startValue: FirstGeneratedId);
            city.Property(c => c.NameEn).HasMaxLength(100);
            city.Property(c => c.NameMr).HasMaxLength(100);
            city.HasIndex(c => c.NameEn).IsUnique();
            city.HasData(
                new City { Id = 1, NameEn = "Pune", NameMr = "पुणे", IsActive = true },
                new City { Id = 2, NameEn = "Mumbai", NameMr = "मुंबई", IsActive = true });
        });

        modelBuilder.Entity<Village>(village =>
        {
            village.Property(v => v.Id).HasIdentityOptions(startValue: FirstGeneratedId);
            village.Property(v => v.NameEn).HasMaxLength(100);
            village.Property(v => v.NameMr).HasMaxLength(100);
            village.HasIndex(v => new { v.CityId, v.NameEn }).IsUnique();
            village.ToTable(t => t.HasCheckConstraint("CK_Villages_DeliveryHours", "\"DeliveryHours\" >= 1"));
            village.HasOne(v => v.City).WithMany(c => c.Villages).HasForeignKey(v => v.CityId).OnDelete(DeleteBehavior.Restrict);
            village.HasData(
                new Village { Id = 1, CityId = 1, NameEn = "Mulshi", NameMr = "मुळशी", DeliveryHours = 3, IsActive = true },
                new Village { Id = 2, CityId = 1, NameEn = "Jejuri", NameMr = "जेजुरी", DeliveryHours = 3, IsActive = true },
                new Village { Id = 3, CityId = 2, NameEn = "Pali", NameMr = "पाली", DeliveryHours = 3, IsActive = true },
                new Village { Id = 4, CityId = 2, NameEn = "Matheran", NameMr = "माथेरान", DeliveryHours = 3, IsActive = true });
        });

        modelBuilder.Entity<Category>(category =>
        {
            category.Property(c => c.Id).HasIdentityOptions(startValue: FirstGeneratedId);
            category.Property(c => c.NameEn).HasMaxLength(100);
            category.Property(c => c.NameMr).HasMaxLength(100);
            category.HasIndex(c => c.NameEn).IsUnique();
            category.HasData(
                new Category { Id = 1, NameEn = "Groceries", NameMr = "किराणा", IsActive = true },
                new Category { Id = 2, NameEn = "Vegetables", NameMr = "भाजीपाला", IsActive = true },
                new Category { Id = 3, NameEn = "Fruits", NameMr = "फळे", IsActive = true },
                new Category { Id = 4, NameEn = "Dairy", NameMr = "दुग्धजन्य पदार्थ", IsActive = true },
                new Category { Id = 5, NameEn = "Household", NameMr = "घरगुती वस्तू", IsActive = true },
                new Category { Id = 6, NameEn = "Personal care", NameMr = "वैयक्तिक काळजी", IsActive = true });
        });
    }
}
