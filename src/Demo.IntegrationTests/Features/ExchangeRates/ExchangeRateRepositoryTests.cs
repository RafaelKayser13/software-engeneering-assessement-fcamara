
namespace Demo.IntegrationTests.Persistence;

public class ExchangeRateRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DemoDbContext _dbContext;
    private readonly ExchangeRateRepository _sut;

    public ExchangeRateRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<DemoDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new DemoDbContext(options);
        _dbContext.Database.EnsureCreated();

        _sut = new ExchangeRateRepository(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task AddRatesAsync_ShouldPersistRates()
    {
        // Arrange
        var rates = new List<ExchangeRate>
        {
            new ExchangeRate { CurrencyCode = "EUR", Rate = 0.85m, FetchedAt = DateTime.UtcNow }
        };

        // Act
        await _sut.AddRatesAsync(rates);

        // Assert
        var savedRate = await _dbContext.ExchangeRates.SingleAsync();
        savedRate.CurrencyCode.Should().Be("EUR");
        savedRate.Rate.Should().Be(0.85m);
    }

    [Fact]
    public async Task GetLatestRateAsync_ShouldReturnMostRecentRate()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var rates = new List<ExchangeRate>
        {
            new ExchangeRate { CurrencyCode = "EUR", Rate = 0.85m, FetchedAt = now.AddDays(-2) },
            new ExchangeRate { CurrencyCode = "EUR", Rate = 0.90m, FetchedAt = now },
            new ExchangeRate { CurrencyCode = "EUR", Rate = 0.88m, FetchedAt = now.AddDays(-1) },
            new ExchangeRate { CurrencyCode = "CAD", Rate = 1.25m, FetchedAt = now }
        };
        await _sut.AddRatesAsync(rates);

        // Act
        var latest = await _sut.GetLatestRateAsync("EUR");

        // Assert
        latest.Should().NotBeNull();
        latest!.Rate.Should().Be(0.90m);
    }
}
