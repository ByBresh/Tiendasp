using System.ComponentModel.DataAnnotations;

namespace Tiendasp.API.Identity.Dto.User
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Old password is required")]
        public string OldPassword { get; set; } = null!;

        [Required(ErrorMessage = "New password is required")]
        public string NewPassword { get; set; } = null!;
    }
}
