using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Tiendasp.API.Identity.Dto.User;

namespace Tiendasp.API.Identity.MinimalApis
{
    public static class UserApi
    {
        public static RouteGroupBuilder MapUserApiEndpoints(this RouteGroupBuilder groups)
        {
            groups.MapGet("me", GetCurrentUserAsync).WithName("Get current user");
            groups.MapPatch("me/change-password", ChangePasswordAsync).WithName("Change password");
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
    }
}
