using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VbMerchant.Data;
using VbMerchant.Models;
using VbMerchant.Services;

namespace VbMerchant.Controllers;


[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DokumanController : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".png", ".jpg", ".jpeg"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf", "image/png", "image/jpeg"
    };

    private readonly AppDbContext _context;
    private readonly ISupabaseStorageService _storageService;

    public DokumanController(AppDbContext context, ISupabaseStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
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

        var sonuclar = new List<string>();

        foreach (var dosya in dosyalar)
        {
            var validationError = ValidateFile(dosya);
            if (validationError is not null)
                return BadRequest(validationError);

            var guvenliDosyaAdi = SanitizeFileName(dosya.FileName);
            var benzersizAd = $"{Guid.NewGuid():N}_{guvenliDosyaAdi}";
            var objectPath = $"basvurular/{basvuruId}/{dokumanTipi}/{benzersizAd}";

            await using var stream = dosya.OpenReadStream();
            await _storageService.UploadAsync(objectPath, stream, dosya.ContentType, HttpContext.RequestAborted);

            _context.BasvuruDokumanlaris.Add(new BasvuruDokumanlari
            {
                BasvuruId = basvuruId,
                DokumanTipi = dokumanTipi,
                DosyaAdi = dosya.FileName,
                DosyaYolu = objectPath,
                DosyaBoyutu = dosya.Length,
                YuklemeTarihi = DateTime.Now
            });

            sonuclar.Add($"{dosya.FileName}: yüklendi.");
        }

        await _context.SaveChangesAsync();
        return Ok(sonuclar);
    }

    private string? ValidateFile(IFormFile dosya)
    {
        if (dosya.Length <= 0)
            return $"{dosya.FileName}: Boş dosya yüklenemez.";

        if (dosya.Length > _storageService.MaxFileSizeBytes)
            return $"{dosya.FileName}: Dosya boyutu sınırı aşıldı.";

        var extension = Path.GetExtension(dosya.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            return $"{dosya.FileName}: Yalnızca PDF, PNG, JPG veya JPEG dosyaları yüklenebilir.";

        if (string.IsNullOrWhiteSpace(dosya.ContentType) || !AllowedContentTypes.Contains(dosya.ContentType))
            return $"{dosya.FileName}: Dosya türü desteklenmiyor.";

        return null;
    }

    private static string SanitizeFileName(string fileName)
    {
        var onlyFileName = Path.GetFileName(fileName);
        var sanitized = Regex.Replace(onlyFileName, @"[^A-Za-z0-9._-]+", "_");
        sanitized = sanitized.Trim('_', '.');

        return string.IsNullOrWhiteSpace(sanitized)
            ? "dokuman"
            : sanitized;
    }
}
