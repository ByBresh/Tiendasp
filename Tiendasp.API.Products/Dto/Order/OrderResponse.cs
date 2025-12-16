namespace Tiendasp.API.Products.Dto.Order
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = null!;
        public List<OrderItemResponse> OrderItems { get; set; } = [];
    }
}
