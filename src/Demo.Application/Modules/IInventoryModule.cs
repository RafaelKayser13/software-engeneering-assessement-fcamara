using Demo.Domain.Modules.Inventory;

namespace Demo.Application.Modules
{
    public interface IInventoryModule
    {
        Task<Product?> GetProduct(string sku, CancellationToken cancellationToken = default);
    }
}

