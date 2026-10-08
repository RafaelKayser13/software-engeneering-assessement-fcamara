namespace Demo.Domain.Features.Inventory
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default);
        Task<Product?> GetProduct(string sku, CancellationToken cancellationToken = default);
    }
}
