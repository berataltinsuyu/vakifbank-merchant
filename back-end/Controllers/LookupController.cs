using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VbMerchant.Data;

namespace VbMerchant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupController : ControllerBase
{
    private readonly AppDbContext _context;

    public LookupController(AppDbContext context)
    {
        _context = context;
    }

    
    [HttpGet("iller")]
    public async Task<IActionResult> GetIller()
    {
        var iller = await _context.Illers
            .OrderBy(i => i.IlAdi)
            .Select(i => new { i.Id, i.IlAdi })
            .ToListAsync();
        return Ok(iller);
    }


    [HttpGet("ilceler/{ilId}")]
    public async Task<IActionResult> GetIlceler(int ilId)
    {
        var ilceler = await _context.Ilcelers
            .Where(i => i.IlId == ilId)
            .OrderBy(i => i.IlceAdi)
            .Select(i => new { i.Id, i.IlceAdi, i.IlId })
            .ToListAsync();
        return Ok(ilceler);
    }

    
    [HttpGet("sirkettipleri")]
    public async Task<IActionResult> GetSirketTipleri()
    {
        var tipler = await _context.SirketTipleris
            .Select(s => new { s.Id, s.TipAdi, s.TipKodu })
            .ToListAsync();
        return Ok(tipler);
    }
}