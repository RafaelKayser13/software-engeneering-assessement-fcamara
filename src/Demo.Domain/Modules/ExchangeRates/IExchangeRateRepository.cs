namespace Demo.Domain.Modules.ExchangeRates;

public interface IExchangeRateRepository
{
    Task AddRatesAsync(IEnumerable<ExchangeRate> rates, CancellationToken cancellationToken = default);
    Task<ExchangeRate?> GetLatestRateAsync(string currencyCode, CancellationToken cancellationToken = default);
}

