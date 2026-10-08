using System.Net;
using System.Text.Json;
using Demo.Infrastructure.Features.ExchangeRates;
using Demo.Infrastructure.Features.ExchangeRates.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace Demo.IntegrationTests.Features.ExchangeRates;

public class OpenExchangeRatesClientTests
{
    [Fact]
    public async Task GetLatestRatesAsync_WithValidConfig_ShouldReturnRates()
    {
        // Arrange
        var configStub = new Dictionary<string, string?>
        {
            {"OpenExchangeRates:ApiKey", "test-key"},
            {"OpenExchangeRates:Symbols:0", "BRL"},
            {"OpenExchangeRates:Symbols:1", "EUR"}
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configStub).Build();

        var mockResponse = new OpenExchangeRatesResponse
        {
            Rates = new Dictionary<string, decimal> { { "BRL", 5.0m }, { "EUR", 0.9m } }
        };

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(mockResponse))
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://test.com/")
        };

        var client = new OpenExchangeRatesClient(httpClient, config);

        // Act
        var rates = await client.GetLatestRatesAsync();

        // Assert
        rates.Should().NotBeNull();
        rates.Should().HaveCount(2);
        rates["BRL"].Should().Be(5.0m);
    }

    [Fact]
    public async Task GetLatestRatesAsync_NoApiKey_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build(); // Empty config
        var httpClient = new HttpClient();
        var client = new OpenExchangeRatesClient(httpClient, config);

        // Act
        var rates = await client.GetLatestRatesAsync();

        // Assert
        rates.Should().BeEmpty();
    }
}

