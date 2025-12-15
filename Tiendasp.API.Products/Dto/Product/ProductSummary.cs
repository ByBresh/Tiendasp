using Tiendasp.API.Products.Dto.Category;

namespace Tiendasp.API.Products.Dto.Product
{
    public class ProductSummary
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public List<CategorySummary> Categories { get; set; } = [];
    }
}
