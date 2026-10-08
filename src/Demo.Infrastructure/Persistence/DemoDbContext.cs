using Demo.Domain.Modules.Inventory;
using Demo.Domain.Modules.ExchangeRates;
using Microsoft.EntityFrameworkCore;

namespace Demo.Infrastructure.Persistence;

public class DemoDbContext : DbContext
{
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ExchangeRate> ExchangeRates { get; set; } = null!;

    public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).ValueGeneratedOnAdd();
            b.HasIndex(p => p.Sku).IsUnique();
            b.Property(p => p.Sku).IsRequired().HasMaxLength(50);
            b.Property(p => p.Name).IsRequired().HasMaxLength(200);
            b.Property(p => p.Price).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<ExchangeRate>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.HasIndex(e => new { e.CurrencyCode, e.FetchedAt });
            b.Property(e => e.CurrencyCode).IsRequired().HasMaxLength(3);
            b.Property(e => e.Rate).HasColumnType("decimal(18,6)");
        });
    }
}
