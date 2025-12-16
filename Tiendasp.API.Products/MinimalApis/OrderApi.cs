using MassTransit.Configuration;
using MassTransit.NewIdProviders;
using MassTransit.Transports;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Security.Claims;
using Tiendasp.API.Products.Dto.Order;
using Tiendasp.API.Products.Entities;

namespace Tiendasp.API.Products.MinimalApis
{
    public static class OrderApi
    {
        public static RouteGroupBuilder MapOrderApiEndpoints(this RouteGroupBuilder groups)
        {
            // Define order-related endpoints here
            groups.MapPost("", CreateOrderAsync).WithName("Create Order").RequireAuthorization("UserOrAdmin");
            groups.MapGet("{id:guid}", GetOrderAsync).WithName("Get Order").RequireAuthorization("UserOrAdmin");
            groups.MapGet("me", GetMyOrdersAsync).WithName("Get My Orders").RequireAuthorization("UserOrAdmin");
            groups.MapPatch("{id:guid}", ConfirmOrder).WithName("Confirm Order").RequireAuthorization("UserOrAdmin");
            return groups;
        }

        public static async Task<IResult> CreateOrderAsync(
            CreateOrderRequest request,
            ClaimsPrincipal principal,
            ProductsDbContext db)
        {
            var productIds = request.OrderItems.Select(i => i.ProductId).ToList();
            if (productIds.Count != productIds.Distinct().Count())
            {
                return Results.BadRequest("Duplicate product IDs are not allowed.");
            }

            var products = await db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var missing = productIds.Except(products.Select(p => p.Id)).ToList();
            if (missing.Count != 0)
            {
                return Results.NotFound(new
                {
                    Message = "Some products were not found",
                    MissingProductIds = missing
                });
            }

            var unavailable = products
                .Where(p => !p.IsActive);

            if (unavailable.Any())
            {
                return Results.Conflict(new
                {
                    Message = "One or more products are not available",
                    Items = unavailable.Select(p => new
                    {
                        p.Id,
                        p.Name
                    })
                });
            }

            var badPrice = products
                .Where(p => p.Price == null);

            if (badPrice.Any())
            {
                return Results.Conflict(new
                {
                    Message = "One or more products have invalid price",
                    Items = badPrice.Select(p => new
                    {
                        p.Id,
                        p.Name
                    })
                });
            }

            var insufficient = products
                .Select(p =>
                {
                    var reqQty = request.OrderItems.First(i => i.ProductId == p.Id).Quantity;
                    return new
                    {
                        ProductId = p.Id,
                        p.Name,
                        p.Stock,
                        Requested = reqQty
                    };
                })
                .Where(x => x.Requested > x.Stock)
                .ToList();

            if (insufficient.Count != 0)
            {
                return Results.Conflict(new
                {
                    Message = "Insufficient stock for one or more products",
                    Items = insufficient.Select(x => new
                    {
                        x.ProductId,
                        x.Name
                    })
                });
            }

            foreach (var item in request.OrderItems)
            {
                var prod = products.First(p => p.Id == item.ProductId);
                prod.Stock -= item.Quantity;
                db.Products.Update(prod);
            }

            var order = new Order
            {
                UserId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!),
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Waiting,
                OrderItems = [.. request.OrderItems.Select(oi => new OrderItem
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = (decimal)products.Find(p => p.Id == oi.ProductId)!.Price // Safe due to previous checks
                })],
                TotalPrice = request.OrderItems.Sum(oi =>
                    oi.Quantity * (decimal)products.Find(p => p.Id == oi.ProductId)!.Price) // Safe due to previous checks
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var orderResponse = new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                Status = order.Status.ToString(),
                OrderItems = [.. order.OrderItems
                    .Select(oi => new OrderItemResponse{
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        UnitPrice = (decimal)oi.Product.Price, // Safe due to previous checks
                        Quantity = oi.Quantity
                    })]
            };
            return Results.CreatedAtRoute("Get Order", new { id = order.Id }, orderResponse);
        }
        public static async Task<IResult> GetOrderAsync(
            Guid id,
            ClaimsPrincipal principal,
            ProductsDbContext db)
        {
            string userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var order = await db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return Results.NotFound();
            }
            if (order.UserId.ToString() != userIdClaim)
            {
                return Results.Forbid();
            }
            var response = new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                Status = order.Status.ToString(),
                OrderItems = [.. order.OrderItems.Select(oi => new OrderItemResponse
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                })]
            };
            return Results.Ok(response);
        }

        public static async Task<IResult> GetMyOrdersAsync(
            ClaimsPrincipal principal,
            ProductsDbContext db)
        {
            string userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var orders = await db.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId.ToString() == userIdClaim)
                .ToListAsync();

            var orderResponses = new List<OrderResponse>();
            foreach (var o in orders)
            {
                var response = new OrderResponse
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status.ToString(),
                    OrderItems = [.. o.OrderItems.Select(oi => new OrderItemResponse
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                })]
                };
                orderResponses.Add(response);
            }
            return Results.Ok(orderResponses);
        }

        public static async Task<IResult> ConfirmOrder(
            Guid id,
            ClaimsPrincipal principal,
            ProductsDbContext db)
        {
            string userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var order = await db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return Results.NotFound();
            }
            if (order.UserId.ToString() != userIdClaim)
            {
                return Results.Forbid();
            }
            if (order.Status != OrderStatus.Waiting)
            {
                return Results.Conflict(new
                {
                    Message = "Order was already confirmed"
                });
            }
            order.Status = OrderStatus.Pending;

            db.Update(order);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }
    }
}
