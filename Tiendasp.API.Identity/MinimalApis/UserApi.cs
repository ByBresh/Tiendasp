using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Http.Resilience;
using System.Security.Claims;
using Tiendasp.API.Identity.Dto.User;

namespace Tiendasp.API.Identity.MinimalApis
{
    public static class UserApi
    {
        public static RouteGroupBuilder MapUserApiEndpoints(this RouteGroupBuilder groups)
        {
            groups.MapGet("me", GetCurrentUserAsync).WithName("Get current user").RequireAuthorization("UserOrAdmin");
            groups.MapPatch("me/change-password", ChangePasswordAsync).WithName("Change password").RequireAuthorization("UserOrAdmin");
            groups.MapPost("{id:guid}/grant-admin", GrantAdminAsync).WithName("Grant admin role").RequireAuthorization("AdminOnly");
            groups.MapPost("{id:guid}/revoke-admin", RevokeAdminAsync).WithName("Revoke admin role").RequireAuthorization("AdminOnly");
            return groups;
        }

        public static async Task<IResult> GetCurrentUserAsync(
            ClaimsPrincipal principal,
            UserManager<IdentityUser> userManager)
        {
            var user = await userManager.GetUserAsync(principal);

            if (user == null)
                return Results.Unauthorized();

            return Results.Ok(new { user.Id, user.UserName, user.Email });
        }

        public static async Task<IResult> ChangePasswordAsync(
            ClaimsPrincipal principal,
            ChangePasswordRequest request,
            UserManager<IdentityUser> userManager,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("UserApi");

            var user = await userManager.GetUserAsync(principal);

            if (user == null)
                return Results.Unauthorized();

            var result = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    logger.LogWarning("Error changing password for user {UserId}: {Error}", user.Id, error.Description);
                }

                return Results.BadRequest(result.Errors.Select(e => e.Description));
            }

            logger.LogInformation("Password changed successfully for user {UserId}.", user.Id);

            return Results.Ok();

        }

        public static async Task<IResult> GrantAdminAsync(
            Guid id,
            UserManager<IdentityUser> userManager,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("UserApi");
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                logger.LogWarning("User with ID {UserId} not found for admin grant.", id);
                return Results.NotFound();
            }
            var result = await userManager.AddToRoleAsync(user, "Admin");
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    logger.LogWarning("Error granting admin role to user {UserId}: {Error}", id, error.Description);
                }
                return Results.BadRequest(result.Errors.Select(e => e.Description));
            }
            logger.LogInformation("Admin role granted to user {UserId}.", id);
            return Results.Ok(result);
        }

        public static async Task<IResult> RevokeAdminAsync(
            ClaimsPrincipal principal,
            Guid id,
            UserManager<IdentityUser> userManager,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("UserApi");
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                logger.LogWarning("User with ID {UserId} not found for admin revoke.", id);
                return Results.NotFound();
            }
            if (user.Id == userManager.GetUserId(principal))
            {
                logger.LogWarning("User {UserId} attempted to revoke their own admin role.", id);
                return Results.BadRequest("You cannot revoke your own admin role.");
            }
            var result = await userManager.RemoveFromRoleAsync(user, "Admin");
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    logger.LogWarning("Error revoking admin role from user {UserId}: {Error}", id, error.Description);
                }
                return Results.BadRequest(result.Errors.Select(e => e.Description));
            }
            logger.LogInformation("Admin role revoked from user {UserId}.", id);
            return Results.Ok(result);
        }
    }
}
