using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VbMerchant.Data;
using VbMerchant.Models;

namespace VbMerchant.Controllers;


[ApiController]
[Route("api/[controller]")]
public class DokumanController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public DokumanController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost("yukle")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(52_428_800)]
    public async Task<IActionResult> Yukle(
        [FromQuery] int basvuruId,
        [FromQuery] string dokumanTipi,
        IFormFileCollection dosyalar)
    {
        if (dosyalar == null || dosyalar.Count == 0)
            return BadRequest("Dosya seçilmedi.");

        var basvuruVarMi = await _context.Basvurulars.AnyAsync(x => x.Id == basvuruId);
        if (!basvuruVarMi)
            return BadRequest("Geçersiz başvuru ID.");

        var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsFolder = Path.Combine(rootPath, "uploads");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var sonuclar = new List<string>();

        foreach (var dosya in dosyalar)
        {
            var benzersizAd = $"{Guid.NewGuid()}_{dosya.FileName}";
            var tamYol = Path.Combine(uploadsFolder, benzersizAd);

            await using (var stream = new FileStream(tamYol, FileMode.Create))
            {
                await dosya.CopyToAsync(stream);
            }

            _context.BasvuruDokumanlaris.Add(new BasvuruDokumanlari
            {
                BasvuruId = basvuruId,
                DokumanTipi = dokumanTipi,
                DosyaAdi = dosya.FileName,
                DosyaYolu = $"/uploads/{benzersizAd}",
                DosyaBoyutu = dosya.Length,
                YuklemeTarihi = DateTime.Now
            });

            sonuclar.Add($"{dosya.FileName}: yüklendi.");
        }

        await _context.SaveChangesAsync();
        return Ok(sonuclar);
    }
}