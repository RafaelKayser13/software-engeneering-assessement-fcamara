using Microsoft.AspNetCore.Mvc;

namespace Demo.Api.Features.Inventory;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IInventoryModule _inventoryModule;

    public ProductsController(IInventoryModule inventoryModule)
    {
        _inventoryModule = inventoryModule;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts([FromQuery] string? currency, CancellationToken cancellationToken)
    {
        try
        {
            var products = await _inventoryModule.GetAllProductsAsync(currency, cancellationToken);
            return Ok(products);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{sku}")]
    public async Task<IActionResult> GetProduct(string sku, CancellationToken cancellationToken)
    {
        var product = await _inventoryModule.GetProduct(sku, cancellationToken);

        if (product is null)
            return NotFound(new { error = $"Product with SKU '{sku}' was not found." });

        return Ok(product);
    }
}
