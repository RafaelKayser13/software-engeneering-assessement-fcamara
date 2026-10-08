
namespace Demo.Infrastructure.Features.Inventory;

public class ProductRepository : IProductRepository
{
    private readonly DemoDbContext _dbContext;

    public ProductRepository(DemoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products.ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetProduct(string sku, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
    }
}
