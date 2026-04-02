namespace VbMerchant.DTOs;
public class BasvuruResponse
{
public int Id { get; set; }
public string SirketTipi { get; set; } = null!; 
public string? FirmaAdi { get; set; }
public string? AdSoyad { get; set; }
public string VergiNoTCKN { get; set; } = null!;
public string? VergiDairesi { get; set; }
public string? YetkiliAdSoyad { get; set; }
public string? CepTelefon { get; set; }
public string Email { get; set; } = null!;
public string Adres { get; set; } = null!;
public string IlAdi { get; set; } = null!; 
public string IlceAdi { get; set; } = null!; 
public string? PostaKodu { get; set; }
public string? WebAdres { get; set; }
public string? IsKategorisi { get; set; }
public decimal? TahminiAylikCiro { get; set; }
public string Durum { get; set; } = null!;
public double? Enlem { get; set; }
public double? Boylam { get; set; }
public DateTime OlusturmaTarihi { get; set; }
public List<DokumanResponse> Dokumanlar { get; set; } = new();
}
public class DokumanResponse
{
public int Id { get; set; }
public string DokumanTipi { get; set; } = null!;
public string DosyaAdi { get; set; } = null!;
public string DosyaYolu { get; set; } = null!;
public long DosyaBoyutu { get; set; }
}