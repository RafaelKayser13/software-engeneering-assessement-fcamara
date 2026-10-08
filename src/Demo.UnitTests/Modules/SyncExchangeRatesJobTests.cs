using Demo.Application.Modules;
using Demo.Domain.Modules.ExchangeRates;
using FluentAssertions;
using Moq;
using Xunit;

namespace Demo.UnitTests.Modules;

public class SyncExchangeRatesJobTests
{
    private readonly Mock<IExchangeRateApiClient> _apiClientMock;
    private readonly Mock<IExchangeRateRepository> _repositoryMock;
    private readonly SyncExchangeRatesJob _sut;

    public SyncExchangeRatesJobTests()
    {
        _apiClientMock = new Mock<IExchangeRateApiClient>();
        _repositoryMock = new Mock<IExchangeRateRepository>();
        _sut = new SyncExchangeRatesJob(_apiClientMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsRates_ShouldSaveToRepository()
    {
        // Arrange
        var rates = new Dictionary<string, decimal>
        {
            { "EUR", 0.85m },
            { "CAD", 1.25m }
        };
        _apiClientMock.Setup(x => x.GetLatestRatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rates);

        IEnumerable<ExchangeRate>? savedRates = null;
        _repositoryMock.Setup(x => x.AddRatesAsync(It.IsAny<IEnumerable<ExchangeRate>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<ExchangeRate>, CancellationToken>((r, c) => savedRates = r)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync();

        // Assert
        savedRates.Should().NotBeNull();
        savedRates.Should().HaveCount(2);
        savedRates.Should().Contain(x => x.CurrencyCode == "EUR" && x.Rate == 0.85m);
        savedRates.Should().Contain(x => x.CurrencyCode == "CAD" && x.Rate == 1.25m);
        _repositoryMock.Verify(x => x.AddRatesAsync(It.IsAny<IEnumerable<ExchangeRate>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsEmpty_ShouldNotSave()
    {
        // Arrange
        _apiClientMock.Setup(x => x.GetLatestRatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>());

        // Act
        await _sut.ExecuteAsync();

        // Assert
        _repositoryMock.Verify(x => x.AddRatesAsync(It.IsAny<IEnumerable<ExchangeRate>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

