using System.ComponentModel.DataAnnotations;

namespace Tiendasp.API.Products.Dto.Product
{
    public class UpdateProductRequest
    {
        [Required(AllowEmptyStrings = false)]
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public bool IsDisabled { get; set; }
        public List<Guid> CategoryIds { get; set; } = [];
    }
}
