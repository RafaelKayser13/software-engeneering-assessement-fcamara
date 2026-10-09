

namespace Demo.Application.Features.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly ILogger<InventoryService> _logger;
        private readonly IProductRepository _productRepository;
        private readonly IExchangeRateRepository _exchangeRateRepository;

        public InventoryService(
            ILogger<InventoryService> logger,
            IProductRepository productRepository,
            IExchangeRateRepository exchangeRateRepository
        )
        {
            _logger = logger;
            _productRepository = productRepository;
            _exchangeRateRepository = exchangeRateRepository;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(string? currency, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all products. Target currency: {Currency}", currency ?? "USD");
            var products = await _productRepository.GetAllProductsAsync(cancellationToken);

            decimal multiplier = 1m;
            string targetCurrency = "USD";

            if (!string.IsNullOrWhiteSpace(currency) && !currency.Equals("USD", StringComparison.OrdinalIgnoreCase))
            {
                var rate = await _exchangeRateRepository.GetLatestRateAsync(currency.ToUpperInvariant(), cancellationToken);
                if (rate == null)
                {
                    _logger.LogWarning("Currency '{Currency}' is not supported or rate not found.", currency);
                    throw new ArgumentException($"Currency '{currency}' is not supported or rates are not available.");
                }

                multiplier = rate.Rate;
                targetCurrency = rate.CurrencyCode;
            }

            _logger.LogInformation("Returning {Count} products.", products.Count());
            return products.Select(p => new ProductDto
            {
                Sku = p.Sku,
                Name = p.Name,
                Price = Math.Round(p.Price * multiplier, 2),
                Currency = targetCurrency
            });
        }

        public async Task<ProductDto?> GetProduct(string sku, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting product with SKU: {Sku}", sku);
            var product = await _productRepository.GetProduct(sku, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product with SKU {Sku} not found.", sku);
                return null;
            }

            return new ProductDto
            {
                Sku = product.Sku,
                Name = product.Name,
                Price = product.Price,
                Currency = "USD"
            };
        }
    }
}
