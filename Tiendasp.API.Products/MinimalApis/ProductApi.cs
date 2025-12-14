using Microsoft.EntityFrameworkCore;
using Tiendasp.API.Products.Dto.Product;
using Tiendasp.API.Products.Entities;

namespace Tiendasp.API.Products.MinimalApis
{
    public static class ProductApi
    {

        public static RouteGroupBuilder MapProductApiEndpoints(this RouteGroupBuilder groups)
        {
            groups.MapPost("", CreateProductAsync).WithName("Create product");
            groups.MapGet("{id:guid}", GetProductAsync).WithName("Get Product");
            groups.MapPut("{id:guid}", UpdateProductAsync).WithName("Update Product");
            groups.MapPatch("{id:guid}/stock", AdjustStockAsync).WithName("Adjust Stock");
            return groups;
        }

        public static async Task<IResult> CreateProductAsync(
            CreateProductRequest request,
            ProductsDbContext db)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                IsDisabled = request.IsDisabled || request.Stock < 1 || request.Price == null,
            }; // TODO: Handle Image Upload

            db.Products.Add(product);
            await db.SaveChangesAsync();

            var response = new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                IsDisabled = product.IsDisabled,
                ImageUrl = product.ImageUrl,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                IsActive = product.IsActive,
            };

            return Results.CreatedAtRoute("Get Product", new { id = product.Id }, response);
        }

        public static async Task<IResult> GetProductAsync(
            Guid id,
            ProductsDbContext db)
        {
            var product = await db.Products.FindAsync(id);
            if (product == null)
                return Results.NotFound();
            var response = new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                IsDisabled = product.IsDisabled,
                ImageUrl = product.ImageUrl,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                IsActive = product.IsActive,
            };
            return Results.Ok(response);
        }

        public static async Task<IResult> UpdateProductAsync(
            Guid id,
            UpdateProductRequest request,
            ProductsDbContext db)
        {
            var product = await db.Products.FindAsync(id);
            if (product == null)
                return Results.NotFound();
            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.IsDisabled = request.IsDisabled || product.Stock < 1 || product.Price == null;
            product.UpdatedAt = DateTime.UtcNow;
            // TODO: Handle Image Upload

            await db.SaveChangesAsync();
            return Results.NoContent();
        }

        public static async Task<IResult> AdjustStockAsync(
            Guid id,
            AdjustStockRequest request,
            ProductsDbContext db)
        {
            // Atomic update to handle concurrency
            var rowsAffected = await db.Products
                .Where(p => p.Id == id && p.Stock + request.Quantity >= 0)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.Stock, p => p.Stock + request.Quantity));

            if (rowsAffected == 0)
            {
                var exists = await db.Products.AnyAsync(p => p.Id == id);
                return exists
                    ? Results.Conflict(new { Message = "Insufficient stock" })
                    : Results.NotFound();
            }

            return Results.NoContent();
        }
    }
}