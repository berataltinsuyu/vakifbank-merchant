using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VbMerchant.DTOs;
using VbMerchant.Repositories;

namespace VbMerchant.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepository, IConfiguration config)
    {
        _userRepository = userRepository;
        _config  = config;
    }

    public async Task<LoginResponse?> GirisAsync(LoginRequest request)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        if (normalizedEmail is null || string.IsNullOrWhiteSpace(request.Sifre))
        {
            return null;
        }

        var kullanici = await _userRepository.GetActiveByEmailAsync(normalizedEmail);

        if (kullanici == null) return null;

        var sifreGecerli = SifreDogrula(request.Sifre, kullanici.SifreHash);
        if (!sifreGecerli) return null;

        kullanici.SonGirisTarihi = DateTime.Now;
        await _userRepository.SaveChangesAsync();

  
        var token = TokenUret(kullanici);

        return new LoginResponse
        {
            Token    = token.Token,
            AdSoyad  = kullanici.AdSoyad,
            Email    = kullanici.Email,
            Role      = kullanici.Rol,
            Expiry   = token.Expiry
        };
    }

    
    public static string SifreHashle(string sifre)
    {
        if (string.IsNullOrWhiteSpace(sifre))
        {
            throw new ArgumentException("Şifre boş olamaz.", nameof(sifre));
        }

        return BCrypt.Net.BCrypt.HashPassword(sifre);
    }

    public static bool SifreDogrula(string sifre, string? sifreHash)
    {
        if (string.IsNullOrWhiteSpace(sifre) || string.IsNullOrWhiteSpace(sifreHash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(sifre, sifreHash);
        }
        catch
        {
            return false;
        }
    }

    private (string Token, DateTime Expiry) TokenUret(Data.Entities.Kullanicilar kullanici)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry  = DateTime.Now.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
            new Claim(ClaimTypes.Email, kullanici.Email),
            new Claim(ClaimTypes.Name, kullanici.AdSoyad),
            new Claim(ClaimTypes.Role, kullanici.Rol),
        };

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            expiry,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }

    private static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim().ToLowerInvariant();
    }
}
