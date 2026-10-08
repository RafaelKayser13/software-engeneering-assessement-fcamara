
namespace Demo.Infrastructure;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(DemoDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!db.Products.Any())
        {
            db.Products.AddRange(
                new Product { Sku = "SKU1", Name = "Product One", Price = 103.30M },
                new Product { Sku = "SKU2", Name = "Product Two", Price = 102.20M }
            );

            await db.SaveChangesAsync();
        }
    }
}
