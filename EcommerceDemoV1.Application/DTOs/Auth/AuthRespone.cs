namespace EcommerceDemoV1.Application.DTOs.Auth
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = null!;
        public object infoUser { get; set; } = default!;
    }
}