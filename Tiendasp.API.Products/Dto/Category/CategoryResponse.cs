using Tiendasp.API.Products.Dto.Product;

namespace Tiendasp.API.Products.Dto.Category
{
    public class CategoryResponse
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
