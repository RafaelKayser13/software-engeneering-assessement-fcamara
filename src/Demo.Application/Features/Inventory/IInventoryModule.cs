

namespace Demo.Application.Features.Inventory
{
    public interface IInventoryModule
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(string? currency, CancellationToken cancellationToken = default);
        Task<ProductDto?> GetProduct(string sku, CancellationToken cancellationToken = default);
    }
}
