namespace Demo.Infrastructure.Clients.Models;

public class OpenExchangeRatesResponse
{
    public Dictionary<string, decimal> Rates { get; set; } = new();
}

