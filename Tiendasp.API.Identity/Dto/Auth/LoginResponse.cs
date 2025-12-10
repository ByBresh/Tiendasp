namespace Tiendasp.API.Identity.Dto.Auth
{
    public class LoginResponse
    {
        public required string Token { get; set; }
        public DateTime ExpirationAtUtc { get; set; }
    }
}
