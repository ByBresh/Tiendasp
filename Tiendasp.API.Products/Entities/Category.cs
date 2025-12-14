namespace Tiendasp.API.Products.Entities;

public class Category
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<ProductCategory> ProductCategories { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];  // Skip navigation
}