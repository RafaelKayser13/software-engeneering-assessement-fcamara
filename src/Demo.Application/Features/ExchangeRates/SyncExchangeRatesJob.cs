
namespace Demo.Application.Features.ExchangeRates;

public class SyncExchangeRatesJob
{
    private readonly IExchangeRateApiClient _apiClient;
    private readonly IExchangeRateRepository _repository;
    private readonly ILogger<SyncExchangeRatesJob> _logger;

    public SyncExchangeRatesJob(
        IExchangeRateApiClient apiClient,
        IExchangeRateRepository repository,
        ILogger<SyncExchangeRatesJob> logger)
    {
        _apiClient = apiClient;
        _repository = repository;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Exchange Rates Sync Job...");
        var rates = await _apiClient.GetLatestRatesAsync(cancellationToken);
        
        if (!rates.Any())
        {
            _logger.LogWarning("No rates retrieved from the API.");
            return;
        }

        var now = DateTime.UtcNow;
        var entities = rates.Select(kv => new ExchangeRate
        {
            CurrencyCode = kv.Key,
            Rate = kv.Value,
            FetchedAt = now
        });

        await _repository.AddRatesAsync(entities, cancellationToken);
        _logger.LogInformation("Successfully synced {Count} exchange rates.", entities.Count());
    }
}
