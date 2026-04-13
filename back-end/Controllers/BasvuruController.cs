using Microsoft.AspNetCore.Mvc;
using VbMerchant.DTOs;
using VbMerchant.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VbMerchant.Data;

namespace VbMerchant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BasvuruController : ControllerBase
{
    private readonly IBasvuruService _service;
    private readonly IValidator<BasvuruCreateRequest> _validator;
    private readonly AppDbContext _context;

    public BasvuruController(
        IBasvuruService service,
        IValidator<BasvuruCreateRequest> validator,
        AppDbContext context)
    {
        _service   = service;
        _validator = validator;
        _context   = context;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound($"Id={id} bulunamadı.");
        return Ok(result);
    }


    [HttpGet("vergino-kontrol")]
    public async Task<IActionResult> VergiNoKontrol([FromQuery] string vergiNo)
    {
        if (string.IsNullOrWhiteSpace(vergiNo))
            return BadRequest(new { mesaj = "Vergi no boş olamaz." });

        var mevcut = await _context.Basvurulars
            .AnyAsync(b => b.VergiNoTckn == vergiNo);

        return Ok(new { mevcut });
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BasvuruCreateRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                ));
        }


        var mevcutMu = await _context.Basvurulars
            .AnyAsync(b => b.VergiNoTckn == request.VergiNoTCKN);
        if (mevcutMu)
            return BadRequest(new { message = "Bu vergi numarasıyla daha önce başvuru yapılmış." });

        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }


    [HttpPatch("{id}/durum")]
    public async Task<IActionResult> UpdateDurum(
        int id,
        [FromQuery] string yeniDurum,
        [FromQuery] string kullanici)
    {
        try
        {
            await _service.UpdateDurumAsync(id, yeniDurum, kullanici);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }


    [HttpGet("liste")]
    public async Task<IActionResult> GetListe()
    {
        var liste = await _context.Basvurulars
            .Include(b => b.Il)
            .Include(b => b.Ilce)
            .Include(b => b.SirketTipi)
            .Select(b => new
            {
                b.Id,
                b.FirmaAdi,
                b.AdSoyad,
                b.VergiNoTckn,
                b.Email,
                b.Durum,
                b.OlusturmaTarihi,
                IlAdi   = b.Il != null ? b.Il.IlAdi : null,
                IlceAdi = b.Ilce != null ? b.Ilce.IlceAdi : null,
                SirketTipi = b.SirketTipi != null ? b.SirketTipi.TipAdi : null,
            })
            .OrderByDescending(b => b.OlusturmaTarihi)
            .AsNoTracking()   
            .ToListAsync();

        return Ok(liste);
    }

    [HttpGet("{id}/detay")]
    public async Task<IActionResult> GetDetay(int id)
    {
        var basvuru = await _context.Basvurulars
            .Include(b => b.SirketTipi)
            .Include(b => b.Il)
            .Include(b => b.Ilce)
            .Include(b => b.BasvuruDokumanlaris)
            .Include(b => b.BasvuruTarihces)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (basvuru == null) return NotFound();

        return Ok(new
        {
            basvuru.Id,
            SirketTipi = basvuru.SirketTipi?.TipAdi,
            basvuru.FirmaAdi,
            basvuru.AdSoyad,
            VergiNoTCKN = basvuru.VergiNoTckn,
            basvuru.VergiDairesi,
            basvuru.YetkiliTckn,
            basvuru.YetkiliAdSoyad,
            basvuru.CepTelefon,
            basvuru.EvTelefon,
            basvuru.IsTelefon,
            basvuru.Email,
            basvuru.Adres,
            IlAdi = basvuru.Il?.IlAdi,
            IlceAdi = basvuru.Ilce?.IlceAdi,
            basvuru.PostaKodu,
            basvuru.WebAdres,
            basvuru.IsKategorisi,
            basvuru.TahminiAylikCiro,
            basvuru.Durum,
            basvuru.Enlem,
            basvuru.Boylam,
            basvuru.OlusturmaTarihi,
            Dokumanlar = basvuru.BasvuruDokumanlaris.Select(d => new
            {
                d.Id, d.DokumanTipi, d.DosyaAdi, d.DosyaYolu, d.DosyaBoyutu
            }),
            BasvuruTarihces = basvuru.BasvuruTarihces.Select(t => new
            {
                t.Durum, t.Aciklama, t.IslemTarihi, t.Kullanici
            })
        });
    }
}