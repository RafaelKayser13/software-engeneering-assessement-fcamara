using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Demo.E2ETests.Features.Inventory;

public class ProductsControllerE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProductsControllerE2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private WebApplicationFactory<Program> CreateTestFactory(SqliteConnection connection)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<DemoDbContext>));
                services.AddDbContext<DemoDbContext>(options => options.UseSqlite(connection));
            });
        });
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnProductsFromDatabase()
    {
        // Arrange — DB is seeded by DatabaseSeeder during app startup (SKU1, SKU2)
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var factory = CreateTestFactory(connection);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();

        products.Should().NotBeNull();
        products.Should().HaveCount(2);
        products!.Should().Contain(p => p.Sku == "SKU1");
        products!.Should().Contain(p => p.Sku == "SKU2");

        connection.Dispose();
    }

    [Fact]
    public async Task GetProductBySku_WhenExists_ShouldReturnProduct()
    {
        // Arrange
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var factory = CreateTestFactory(connection);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products/SKU2");

        // Assert
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();

        product.Should().NotBeNull();
        product!.Sku.Should().Be("SKU2");

        connection.Dispose();
    }

    [Fact]
    public async Task GetProductBySku_WhenNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var factory = CreateTestFactory(connection);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products/NOT-FOUND");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        connection.Dispose();
    }
}
