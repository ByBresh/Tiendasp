namespace Tiendasp.API.Products.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalPrice { get; set; }
    public OrderStatus Status { get; set; } = 0;
    // Navigation
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}

public enum OrderStatus
{
    Waiting = 0,
    Pending = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}
