using Microsoft.AspNetCore.Mvc;

namespace Demo.Api.Features.Inventory;

[ApiController]
[Route("api/products")]
[Tags("Product Catalog")]
public class ProductsController : ControllerBase
{
    private readonly IInventoryService _InventoryService;

    public ProductsController(IInventoryService InventoryService)
    {
        _InventoryService = InventoryService;
    }

    /// <summary>
    /// Gets all available products.
    /// </summary>
    /// <param name="currency">Optional currency code to convert prices (e.g. EUR, CAD). If omitted, returns USD.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of products with their current prices.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllProducts([FromQuery] string? currency, CancellationToken cancellationToken)
    {
        try
        {
            var products = await _InventoryService.GetAllProductsAsync(currency, cancellationToken);
            return Ok(products);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific product by its SKU.
    /// </summary>
    /// <param name="sku">The unique Stock Keeping Unit identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested product.</returns>
    [HttpGet("{sku}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(string sku, CancellationToken cancellationToken)
    {
        var product = await _InventoryService.GetProduct(sku, cancellationToken);

        if (product is null)
            return NotFound(new { error = $"Product with SKU '{sku}' was not found." });

        return Ok(product);
    }
}
