namespace Demo.Domain.Modules.ExchangeRates;

public interface IExchangeRateApiClient
{
    Task<Dictionary<string, decimal>> GetLatestRatesAsync(CancellationToken cancellationToken = default);
}

