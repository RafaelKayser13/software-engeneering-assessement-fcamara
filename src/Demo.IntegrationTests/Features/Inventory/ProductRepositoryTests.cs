using Demo.Domain.Features.Inventory;
using Demo.Infrastructure.Data;
using Demo.Infrastructure.Features.Inventory;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Demo.IntegrationTests.Features.Inventory;

public class ProductRepositoryTests
{
    private DbContextOptions<DemoDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<DemoDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var options = CreateNewContextOptions();
        await using var context = new DemoDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        context.Products.Add(new Product { Sku = "SKU1", Name = "Product 1", Price = 10.0m });
        context.Products.Add(new Product { Sku = "SKU2", Name = "Product 2", Price = 20.0m });
        await context.SaveChangesAsync();

        var repo = new ProductRepository(context);
        
        // Act
        var products = await repo.GetAllProductsAsync();
        
        // Assert
        products.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProduct_ShouldReturnProduct_WhenExists()
    {
        // Arrange
        var options = CreateNewContextOptions();
        await using var context = new DemoDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        context.Products.Add(new Product { Sku = "SKU1", Name = "Product 1", Price = 10.0m });
        await context.SaveChangesAsync();

        var repo = new ProductRepository(context);
        
        // Act
        var product = await repo.GetProduct("SKU1");
        
        // Assert
        product.Should().NotBeNull();
        product!.Sku.Should().Be("SKU1");
    }
}

