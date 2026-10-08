using Demo.Domain.Modules.ExchangeRates;

namespace Demo.Application.Modules;

public class SyncExchangeRatesJob
{
    private readonly IExchangeRateApiClient _apiClient;
    private readonly IExchangeRateRepository _repository;

    public SyncExchangeRatesJob(IExchangeRateApiClient apiClient, IExchangeRateRepository repository)
    {
        _apiClient = apiClient;
        _repository = repository;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var rates = await _apiClient.GetLatestRatesAsync(cancellationToken);
        if (!rates.Any()) return;

        var now = DateTime.UtcNow;
        var entities = rates.Select(kv => new ExchangeRate
        {
            CurrencyCode = kv.Key,
            Rate = kv.Value,
            FetchedAt = now
        });

        await _repository.AddRatesAsync(entities, cancellationToken);
    }
}

