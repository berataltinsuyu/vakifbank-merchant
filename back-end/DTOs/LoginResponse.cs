namespace VbMerchant.DTOs
{
    public class LoginResponse
    {
        
        public string Email { get; set; } = null!;

        public string AdSoyad   { get; set; } = null!;

        public string Token { get; set; } = null!;

        public string Role { get; set; } = null!;

        public DateTime Expiry { get; set; }

        
    }
} 