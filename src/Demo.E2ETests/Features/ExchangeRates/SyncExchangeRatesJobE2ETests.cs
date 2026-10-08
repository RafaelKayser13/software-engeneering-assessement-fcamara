using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Demo.E2ETests.Jobs;

public class SyncExchangeRatesJobE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SyncExchangeRatesJobE2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SyncJob_ShouldFetchFromApiAndSaveToDb()
    {
        // Arrange
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var clientFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove real DB and replace with test in-memory SQLite DB
                services.RemoveAll(typeof(DbContextOptions<DemoDbContext>));
                services.AddDbContext<DemoDbContext>(options => options.UseSqlite(connection));
                
                // Swap external API client with a stub to avoid hitting real network in E2E
                services.RemoveAll(typeof(IExchangeRateApiClient));
                services.AddScoped<IExchangeRateApiClient, StubApiClient>();
            });
        });

        // Ensure schema is created in our test DB
        using var scope = clientFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
        db.Database.EnsureCreated();

        var job = scope.ServiceProvider.GetRequiredService<SyncExchangeRatesJob>();

        // Act
        await job.ExecuteAsync();

        // Assert
        var savedRates = await db.ExchangeRates.ToListAsync();
        savedRates.Should().HaveCount(1);
        savedRates.First().CurrencyCode.Should().Be("EUR");
        savedRates.First().Rate.Should().Be(0.99m);

        connection.Dispose();
    }

    private class StubApiClient : IExchangeRateApiClient
    {
        public Task<Dictionary<string, decimal>> GetLatestRatesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Dictionary<string, decimal> { { "EUR", 0.99m } });
        }
    }
}
