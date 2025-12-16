using System.ComponentModel.DataAnnotations;

namespace Tiendasp.API.Products.Dto.Order
{
    public class OrderRequestItems
    {
        [Required(ErrorMessage = "ProductId is required")]
        public Guid ProductId { get; set; }
        [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50")]
        public int Quantity { get; set; }
    }
}
