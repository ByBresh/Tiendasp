namespace Tiendasp.API.Products.Entities;

public class Product
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int Stock { get; set; } = 0;
    public bool IsDisabled { get; set; } = true;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive => Price != null && Stock > 0 && !IsDisabled;

    // Navigation
    public ICollection<ProductCategory> ProductCategories { get; set; } = [];
    public ICollection<Category> Categories { get; set; } = [];  // Skip navigation
}