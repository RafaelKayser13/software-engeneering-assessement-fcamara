using Demo.Api.DTOs;
using Demo.Application.Modules;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IInventoryModule _inventoryModule;

    public ProductsController(IInventoryModule inventoryModule)
    {
        _inventoryModule = inventoryModule;
    }

    [HttpGet("{sku}")]
    public async Task<IActionResult> GetProduct(string sku, CancellationToken cancellationToken)
    {
        var product = await _inventoryModule.GetProduct(sku, cancellationToken);

        if (product is null)
            return NotFound(new { error = $"Product with SKU '{sku}' was not found." });

        return Ok(new ProductResponse
        {
            Sku = product.Sku,
            Name = product.Name,
            Price = product.Price
        });
    }
}
