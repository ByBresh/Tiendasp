using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tiendasp.API.Products;

public class ProductsDbContextFactory : IDesignTimeDbContextFactory<ProductsDbContext>
{
    public ProductsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProductsDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=productsdb;Username=postgres;Password=postgres");
        return new ProductsDbContext(optionsBuilder.Options);
    }
}