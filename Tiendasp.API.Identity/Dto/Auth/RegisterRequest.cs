using System.ComponentModel.DataAnnotations;

namespace Tiendasp.API.Identity.Dto.Auth
{
    public class RegisterRequest
    {
        public required string Email { get; set; }

        public required string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public required string ConfirmPassword { get; set; }
    }
}