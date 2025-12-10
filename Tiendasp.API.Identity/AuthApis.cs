using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tiendasp.API.Identity.Dto.Auth;
using Tiendasp.Shared.Events;

namespace Tiendasp.API.Identity;
public static class AuthApis
{
    public static RouteGroupBuilder MapApiEndpoints(this RouteGroupBuilder groups)
    {
        groups.MapPost("login", LoginAsync).WithName("Login");
        groups.MapPost("register", RegisterAsync).WithName("Register");
        return groups;
    }

    public static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<IdentityUser> userManager,
        IConfiguration configuration)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return Results.Unauthorized();
            }

            var result = await userManager.CheckPasswordAsync(user, request.Password);

            if (!result)
            {
                return Results.Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);

            // Claim - agregar roles al token
            var claims = new List<Claim>
                {
                    new (ClaimTypes.Name, user.UserName!),
                    new (ClaimTypes.Email, user.Email!),
                    new (ClaimTypes.NameIdentifier, user.Id),
                    new (ClaimTypes.Role, roles.FirstOrDefault() ?? "NoRole")
                };

            // Generate JWT Token

            var secretKey = configuration["JWT:SecretKey"];
            var audience = configuration["JWT:Audience"];
            var issuer = configuration["JWT:Issuer"];
            var expirationMinutes = int.Parse(configuration["JWT:ExpiryInMinutes"]!);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: creds
            );

            var encryptedToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Results.Ok(new LoginResponse
            {
                Token = encryptedToken,
                ExpirationAtUtc = DateTime.UtcNow.AddMinutes(expirationMinutes)
            });
        } catch (Exception ex)
        {
            return Results.BadRequest(new {message = ex.Message});
        }
    }
    public static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        UserManager<IdentityUser> userManager,
        IConfiguration configuration,
        IPublishEndpoint publishEndpoint)
    {
        try
        {
            var result = await userManager.CreateAsync(new IdentityUser
            {
                UserName = request.Email.Split("@")[0],
                Email = request.Email
            }, request.Password);

            if (!result.Succeeded)
            {
                return Results.ValidationProblem(
                    result.Errors.ToDictionary(
                        e => e.Code,
                        e => new[] { e.Description }
                    )
                );
            }

            var user = await userManager.FindByEmailAsync(request.Email);

            if (result == null || user == null)
                return Results.InternalServerError();

            await publishEndpoint.Publish(new UserCreatedEvent(user.Id, user.Email!));

            var loginResponse = await LoginAsync(new LoginRequest { Email = request.Email, Password = request.Password }, userManager, configuration);

            return Results.Ok(loginResponse);

        }
        catch (Exception)
        {
            return Results.InternalServerError();
        }
    }

}

