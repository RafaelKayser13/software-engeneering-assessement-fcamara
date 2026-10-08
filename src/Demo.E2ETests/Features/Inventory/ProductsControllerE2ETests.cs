using System.Net;
using System.Net.Http.Json;
using Demo.Application.Features.Inventory;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FluentAssertions;

namespace Demo.E2ETests.Features.Inventory;

public class ProductsControllerE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProductsControllerE2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnExpectedData_WhenModuleReturnsProducts()
    {
        // Arrange
        var clientFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IInventoryModule));
                services.AddScoped<IInventoryModule, StubInventoryModule>();
            });
        });
        var client = clientFactory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        
        products.Should().NotBeNull();
        products.Should().HaveCount(1);
        products!.First().Sku.Should().Be("MOCK-SKU");
    }

    [Fact]
    public async Task GetProductBySku_WhenExists_ShouldReturnProduct()
    {
        // Arrange
        var clientFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IInventoryModule));
                services.AddScoped<IInventoryModule, StubInventoryModule>();
            });
        });
        var client = clientFactory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products/MOCK-SKU");

        // Assert
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        
        product.Should().NotBeNull();
        product!.Sku.Should().Be("MOCK-SKU");
    }

    [Fact]
    public async Task GetProductBySku_WhenNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var clientFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IInventoryModule));
                services.AddScoped<IInventoryModule, StubInventoryModule>();
            });
        });
        var client = clientFactory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products/NOT-FOUND");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private class StubInventoryModule : IInventoryModule
    {
        public Task<IEnumerable<ProductDto>> GetAllProductsAsync(string? currency, CancellationToken cancellationToken = default)
        {
            var list = new List<ProductDto>
            {
                new ProductDto { Sku = "MOCK-SKU", Name = "Mock Product", Price = 10m, Currency = "USD" }
            };
            return Task.FromResult<IEnumerable<ProductDto>>(list);
        }

        public Task<ProductDto?> GetProduct(string sku, CancellationToken cancellationToken = default)
        {
            if (sku == "MOCK-SKU")
            {
                return Task.FromResult<ProductDto?>(new ProductDto { Sku = "MOCK-SKU", Name = "Mock Product", Price = 10m, Currency = "USD" });
            }
            return Task.FromResult<ProductDto?>(null);
        }
    }
}
