using Demo.Domain.Modules.Inventory;
using Microsoft.Extensions.Logging;

namespace Demo.Application.Modules
{
    public class InventoryModule : IInventoryModule
    {
        private readonly ILogger<InventoryModule> _logger;
        private readonly IProductRepository _productRepository;

        public InventoryModule(
            ILogger<InventoryModule> logger,
            IProductRepository productRepository
        )
        {
            _logger = logger;
            _productRepository = productRepository;
        }

        public Task<Product?> GetProduct(string sku, CancellationToken cancellationToken = default)
        {
            return _productRepository.GetProduct(sku, cancellationToken);
        }
    }
}
