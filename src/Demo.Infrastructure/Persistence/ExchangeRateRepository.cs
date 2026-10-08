using Demo.Domain.Modules.ExchangeRates;
using Microsoft.EntityFrameworkCore;

namespace Demo.Infrastructure.Persistence;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly DemoDbContext _dbContext;

    public ExchangeRateRepository(DemoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddRatesAsync(IEnumerable<ExchangeRate> rates, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<ExchangeRate>().AddRange(rates);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ExchangeRate?> GetLatestRateAsync(string currencyCode, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ExchangeRate>()
            .Where(x => x.CurrencyCode == currencyCode)
            .OrderByDescending(x => x.FetchedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

