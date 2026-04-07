using Microsoft.AspNetCore.Mvc;
using VbMerchant.DTOs;
using VbMerchant.Services;
using VbMerchant.Data;
using Microsoft.EntityFrameworkCore;

namespace VbMerchant.Controllers;

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("giris")]
    public async Task<IActionResult> Giris([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Sifre))
            return BadRequest("Email ve şifre zorunludur.");

        var sonuc = await _authService.GirisAsync(request);

        if (sonuc == null)
            return Unauthorized("Email veya şifre hatalı.");

        return Ok(sonuc);
    }

    
// [HttpPost("seed")]
// public async Task<IActionResult> Seed([FromServices] AppDbContext db)
// {
//     var hash = AuthService.SifreHashle("123456");
//     var kullanici = await db.Kullanicilars
//         .FirstOrDefaultAsync(k => k.Email == "admin@vakifbank.com.tr");
//     if (kullanici != null)
//     {
//         kullanici.SifreHash = hash;
//         await db.SaveChangesAsync();
//     }
//     return Ok("Şifre güncellendi.");
// }
}