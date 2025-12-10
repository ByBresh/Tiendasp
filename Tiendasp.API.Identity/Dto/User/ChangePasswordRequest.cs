using System.ComponentModel.DataAnnotations;

namespace Tiendasp.API.Identity.Dto.User
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Old password is required")]
        public required string OldPassword { get; set; }
        
        [Required(ErrorMessage = "New password is required")]
        public required string NewPassword { get; set; }
    }
}
