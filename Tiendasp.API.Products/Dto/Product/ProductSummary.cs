namespace Tiendasp.API.Products.Dto.Product
{
    public class ProductSummary
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public decimal? Price { get; set; }
    }
}
