using System.ComponentModel.DataAnnotations;

namespace Tiendasp.API.Identity.Dto.Auth
{
    public class RegisterRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public required string ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password.Length < 8)
            {
                yield return new ValidationResult("The password must be at least 8 characters", [nameof(Password)]);
            }

            if (Password.Length > 64)
            {
                yield return new ValidationResult("Password can't be longer than 64 characters", [nameof(Password)]);
            }
        
            if (!System.Text.RegularExpressions.Regex.IsMatch(Password, @"[a-z]"))
            {
                yield return new ValidationResult("Password must contain at least one lowercase letter", [nameof(Password)]);
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(Password, @"[A-Z]"))
            {
                yield return new ValidationResult("Password must contain at least one uppercase letter", [nameof(Password)]);
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(Password, @"\d"))
            {
                yield return new ValidationResult("Password must contain at least one number", [nameof(Password)]);
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(Password, @"[@$!%*?&]"))
            {
                yield return new ValidationResult("Password must contain at least one special character (@$!%*?&)", [nameof(Password)]);
            }
        }
    }
}