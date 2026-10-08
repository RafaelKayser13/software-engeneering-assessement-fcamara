using Microsoft.Extensions.Logging;

namespace Demo.UnitTests.Modules;

public class InventoryServiceTests
{
    private readonly Mock<ILogger<InventoryService>> _loggerMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IExchangeRateRepository> _exchangeRepoMock;
    private readonly InventoryService _sut;

    public InventoryServiceTests()
    {
        _loggerMock = new Mock<ILogger<InventoryService>>();
        _productRepoMock = new Mock<IProductRepository>();
        _exchangeRepoMock = new Mock<IExchangeRateRepository>();
        _sut = new InventoryService(_loggerMock.Object, _productRepoMock.Object, _exchangeRepoMock.Object);
    }

    [Fact]
    public async Task GetAllProductsAsync_WhenNoCurrencySpecified_ReturnsUsdPrices()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Sku = "SKU1", Name = "Product 1", Price = 100m }
        };
        _productRepoMock.Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllProductsAsync(null);

        // Assert
        result.Should().HaveCount(1);
        result.First().Currency.Should().Be("USD");
        result.First().Price.Should().Be(100m);
        _exchangeRepoMock.Verify(x => x.GetLatestRateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllProductsAsync_WhenCurrencySpecified_ReturnsConvertedPrices()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Sku = "SKU1", Name = "Product 1", Price = 100m }
        };
        _productRepoMock.Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(products);

        var rate = new ExchangeRate { CurrencyCode = "EUR", Rate = 0.85m, FetchedAt = DateTime.UtcNow };
        _exchangeRepoMock.Setup(x => x.GetLatestRateAsync("EUR", It.IsAny<CancellationToken>())).ReturnsAsync(rate);

        // Act
        var result = await _sut.GetAllProductsAsync("EUR");

        // Assert
        result.Should().HaveCount(1);
        result.First().Currency.Should().Be("EUR");
        result.First().Price.Should().Be(85m);
        _exchangeRepoMock.Verify(x => x.GetLatestRateAsync("EUR", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllProductsAsync_WhenCurrencyIsUnsupported_ThrowsArgumentException()
    {
        // Arrange
        var products = new List<Product>();
        _productRepoMock.Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(products);
        _exchangeRepoMock.Setup(x => x.GetLatestRateAsync("XYZ", It.IsAny<CancellationToken>())).ReturnsAsync((ExchangeRate?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetAllProductsAsync("XYZ"));
    }
}
