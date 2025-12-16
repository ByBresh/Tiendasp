using System.ComponentModel.DataAnnotations;

namespace Tiendasp.API.Products.Dto.Order
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "OrderItems are required")]
        [MinLength(1, ErrorMessage = "At least one order item is required")]
        [MaxLength(20, ErrorMessage = "A maximum of 20 order items are allowed")]
        public List<OrderRequestItems> OrderItems { get; set; } = null!;
    }
}
