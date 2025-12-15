using System.ComponentModel.DataAnnotations;

namespace Tiendasp.API.Products.Dto.Category
{
    public class CreateCategoryRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
