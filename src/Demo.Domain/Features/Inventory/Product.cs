namespace Demo.Domain.Features.Inventory
{
    public class Product
    {
        public int Id { get; set; }
        public required string Sku { get; set; }
        public required string Name { get; set; }
        public required decimal Price { get; set; }
    }
}
