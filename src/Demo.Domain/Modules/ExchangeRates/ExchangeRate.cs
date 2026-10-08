namespace Demo.Domain.Modules.ExchangeRates;

public class ExchangeRate
{
    public int Id { get; set; }
    public required string CurrencyCode { get; set; }
    public required decimal Rate { get; set; }
    public required DateTime FetchedAt { get; set; }
}

