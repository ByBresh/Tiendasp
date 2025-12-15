namespace Tiendasp.API.Products.Dto.Category
{
    public class UpdateCategoryRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
