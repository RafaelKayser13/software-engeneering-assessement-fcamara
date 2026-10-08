namespace Demo.Infrastructure.Features.ExchangeRates.Models;

public class OpenExchangeRatesResponse
{
    public Dictionary<string, decimal> Rates { get; set; } = new();
}
