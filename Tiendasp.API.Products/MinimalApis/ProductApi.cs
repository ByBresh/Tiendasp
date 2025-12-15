using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tiendasp.API.Products.Dto.Category;
using Tiendasp.API.Products.Dto.Product;
using Tiendasp.API.Products.Entities;

namespace Tiendasp.API.Products.MinimalApis
{
    public static class ProductApi
    {

        public static RouteGroupBuilder MapProductApiEndpoints(this RouteGroupBuilder groups)
        {
            groups.MapPost("", CreateProductAsync).WithName("Create product").RequireAuthorization("AdminOnly");
            groups.MapGet("{id:guid}/detailed", GetProductDetailedAsync).WithName("Get Product Detailed").RequireAuthorization("AdminOnly");
            groups.MapPut("{id:guid}", UpdateProductAsync).WithName("Update Product").RequireAuthorization("AdminOnly");
            groups.MapPatch("{id:guid}/stock", AdjustStockAsync).WithName("Adjust Stock").RequireAuthorization("AdminOnly");

            // Client endpoints
            groups.MapGet("list", ListProducts).WithName("List Products");
            return groups;
        }

        public static async Task<IResult> CreateProductAsync(
            CreateProductRequest request,
            ProductsDbContext db)
        {
            var validationError = await ValidateCategoryIdsAsync(request.CategoryIds, db);
            if (validationError is not null)
                return validationError;

            var categories = await db.Categories
                .Where(c => request.CategoryIds.Contains(c.Id))
                .ToListAsync();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                IsDisabled = request.IsDisabled || request.Stock < 1 || request.Price == null,
                Categories = categories,
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
                Categories = [.. product.Categories
                    .Select(pc => new CategorySummary
                    {
                        Id = pc.Id,
                        Name = pc.Name
                    })]
            };

            return Results.CreatedAtRoute("Get Product", new { id = product.Id }, response);
        }

        public static async Task<IResult> GetProductDetailedAsync(
            Guid id,
            ProductsDbContext db)
        {
            var product = await db.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == id);
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
                Categories = [.. product.Categories
                    .Select(pc => new CategorySummary
                    {
                        Id = pc.Id,
                        Name = pc.Name
                    })]
            };
            return Results.Ok(response);
        }

        public static async Task<IResult> UpdateProductAsync(
            Guid id,
            UpdateProductRequest request,
            ProductsDbContext db)
        {
            var validationError = await ValidateCategoryIdsAsync(request.CategoryIds, db);
            if (validationError is not null)
                return validationError;

            var product = await db.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                return Results.NotFound();

            var categories = await db.Categories
                .Where(c => request.CategoryIds.Contains(c.Id))
                .ToListAsync();

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.IsDisabled = request.IsDisabled || product.Stock < 1 || product.Price == null;
            product.Categories = categories;
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

        private static async Task<IResult?> ValidateCategoryIdsAsync(
            List<Guid> categoryIds,
            ProductsDbContext db)
        {
            if (categoryIds.Count == 0)
                return null;

            var existingIds = await db.Categories
                .Where(c => categoryIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            var invalidIds = categoryIds.Except(existingIds).ToList();
            if (invalidIds.Count > 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["CategoryIds"] = [$"Categories not found: {string.Join(", ", invalidIds)}"]
                });
            }

            return null;
        }

        public static async Task<IResult> ListProducts(
            ProductsDbContext db,
            [FromQuery] Guid? cId = null,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool desc = false,
            [FromQuery] string? s = null)
        {
            var query = db.Products
                .AsQueryable();

            if (cId.HasValue)
            {
                query = query
                    .Where(p => p.Categories.Any(c => c.Id == cId.Value));
            }

            query = orderBy?.ToLowerInvariant() switch
            {
                "name" => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "price" => desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "created" => desc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt) // Default: newest first
            };

            if (!string.IsNullOrWhiteSpace(s))
            {
                s = s.Trim().ToLowerInvariant();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(s) ||
                    (p.Description != null && p.Description.ToLower().Contains(s)));
            }

            var products = await query
                .Include(p => p.Categories)
                .ToListAsync();

            var response = products.Select(product => new ProductSummary
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                Categories = [.. product.Categories.Select(c => new CategorySummary
                {
                    Id = c.Id,
                    Name = c.Name
                })]
            }).ToList();
            return Results.Ok(response);
        }
    }
}