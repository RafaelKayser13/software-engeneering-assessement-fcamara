using Demo.Api.Middleware;
using Hangfire;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<DemoDbContext>(options =>
    options.UseSqlite(connectionString));

// Repositories and modules
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IInventoryModule, InventoryModule>();

// Exchange Rates (Task 2)
builder.Services.AddHttpClient<Demo.Domain.Features.ExchangeRates.IExchangeRateApiClient, Demo.Infrastructure.Features.ExchangeRates.OpenExchangeRatesClient>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var baseUrl = config["OpenExchangeRates:BaseUrl"] ?? "https://openexchangerates.org";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddScoped<Demo.Domain.Features.ExchangeRates.IExchangeRateRepository, ExchangeRateRepository>();
builder.Services.AddTransient<SyncExchangeRatesJob>();

// Hangfire
builder.Services.AddInfrastructureHangfire();

var app = builder.Build();

// Global error handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHangfireDashboard();
app.MapControllers();

// Hangfire Jobs
RecurringJob.AddOrUpdate<SyncExchangeRatesJob>("sync-exchange-rates", j => j.ExecuteAsync(CancellationToken.None), Cron.Weekly(DayOfWeek.Monday));

// Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
    await DatabaseSeeder.SeedAsync(db);
}

app.Run();

public partial class Program { }
