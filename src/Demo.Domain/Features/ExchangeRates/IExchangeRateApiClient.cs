namespace Demo.Domain.Features.ExchangeRates;

public interface IExchangeRateApiClient
{
    Task<Dictionary<string, decimal>> GetLatestRatesAsync(CancellationToken cancellationToken = default);
}
