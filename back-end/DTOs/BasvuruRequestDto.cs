namespace VbMerchant.DTOs;
public class BasvuruCreateRequest
{
public int SirketTipiId { get; set; }
public string? FirmaAdi { get; set; } 
public string? AdSoyad { get; set; }
public string VergiNoTCKN { get; set; } = null!;
public string? VergiDairesi { get; set; }
public string? YetkiliTCKN { get; set; }
public string? YetkiliAdSoyad { get; set; }
public string? EvTelefon { get; set; }
public string? IsTelefon { get; set; }
public string? CepTelefon { get; set; }
public string Email { get; set; } = null!;
public string Adres { get; set; } = null!;
public int IlId { get; set; }
public int IlceId { get; set; }
public string? PostaKodu { get; set; }
public string? WebAdres { get; set; }
public string? IsKategorisi { get; set; }
public decimal? TahminiAylikCiro { get; set; }
public double? Enlem { get; set; } 
public double? Boylam { get; set; } 
}