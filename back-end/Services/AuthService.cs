using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VbMerchant.Data;
using VbMerchant.DTOs;
using VbMerchant.Models;

namespace VbMerchant.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config  = config;
    }

    public async Task<LoginResponse?> GirisAsync(LoginRequest request)
    {
  
        var kullanici = await _context.Kullanicilars
            .FirstOrDefaultAsync(k => k.Email == request.Email && k.IsActive == true);

        if (kullanici == null) return null;

      
        var sifreGecerli = BCrypt.Net.BCrypt.Verify(request.Sifre, kullanici.SifreHash);
        if (!sifreGecerli) return null;

        kullanici.SonGirisTarihi = DateTime.Now;
        await _context.SaveChangesAsync();

  
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
        => BCrypt.Net.BCrypt.HashPassword(sifre);

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
}