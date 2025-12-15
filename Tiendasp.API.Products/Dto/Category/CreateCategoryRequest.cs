namespace Tiendasp.API.Products.Dto.Category
{
    public class CreateCategoryRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
