namespace Demo.Application.Features.Inventory;

public class ProductDto
{
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public required string Currency { get; set; }
}
