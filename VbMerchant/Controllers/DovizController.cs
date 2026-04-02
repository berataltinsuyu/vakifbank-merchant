using Microsoft.AspNetCore.Mvc;
using VbMerchant.Services.Concrete;

namespace VbMerchant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DovizController : ControllerBase
{
    private readonly DovizService _service;

    public DovizController(DovizService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var kurlar = await _service.KurlariGetirAsync();
        return Ok(kurlar);
    }
}