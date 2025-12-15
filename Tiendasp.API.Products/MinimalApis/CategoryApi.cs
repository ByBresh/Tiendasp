using Tiendasp.API.Products.Dto.Category;
using Tiendasp.API.Products.Entities;

namespace Tiendasp.API.Products.MinimalApis
{
    public static class CategoryApi
    {
        public static RouteGroupBuilder MapCategoryApiEndpoints(this RouteGroupBuilder groups)
        {
            groups.MapPost("", CreateCategoryAsync).WithName("Create Category").RequireAuthorization("AdminOnly");
            groups.MapGet("{id:guid}", GetCategoryAsync).WithName("Get Category").RequireAuthorization("AdminOnly");
            groups.MapPut("{id:guid}", UpdateCategoryAsync).WithName("Update Category").RequireAuthorization("AdminOnly");
            return groups;
        }

        public static async Task<IResult> CreateCategoryAsync(
            CreateCategoryRequest request,
            ProductsDbContext db)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description
            };
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            var response = new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
            };
            return Results.CreatedAtRoute("Get Category", new { id = category.Id }, response);
        }

        public static async Task<IResult> GetCategoryAsync(
            Guid id,
            ProductsDbContext db)
        {
            var category = await db.Categories.FindAsync(id);
            if (category is null)
            {
                return Results.NotFound();
            }
            var response = new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
            };
            return Results.Ok(response);
        }

        public static async Task<IResult> UpdateCategoryAsync(
            Guid id,
            UpdateCategoryRequest request,
            ProductsDbContext db)
        {
            var category = await db.Categories.FindAsync(id);
            if (category is null)
            {
                return Results.NotFound();
            }
            category.Name = request.Name;
            category.Description = request.Description;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }
    }
}