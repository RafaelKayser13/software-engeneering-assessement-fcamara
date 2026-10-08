using System.Net.Http.Json;
using Demo.Domain.Modules.ExchangeRates;
using Demo.Infrastructure.Clients.Models;
using Microsoft.Extensions.Configuration;

namespace Demo.Infrastructure.Clients;

public class OpenExchangeRatesClient : IExchangeRateApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string[] _symbols;

    public OpenExchangeRatesClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["OpenExchangeRates:ApiKey"] ?? "";
        _symbols = config.GetSection("OpenExchangeRates:Symbols").Get<string[]>() ?? Array.Empty<string>();
    }

    public async Task<Dictionary<string, decimal>> GetLatestRatesAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_apiKey)) return new Dictionary<string, decimal>();

        var url = $"api/latest.json?app_id={_apiKey}";
        if (_symbols.Length > 0)
        {
            url += $"&symbols={string.Join(",", _symbols)}";
        }
        
        var response = await _httpClient.GetFromJsonAsync<OpenExchangeRatesResponse>(url, cancellationToken);
        
        return response?.Rates ?? new Dictionary<string, decimal>();
    }
}

