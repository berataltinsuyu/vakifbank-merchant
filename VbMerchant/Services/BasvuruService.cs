using VbMerchant.Data;
using VbMerchant.DTOs;
using VbMerchant.Models;
using VbMerchant.Repositories;
using VbMerchant.Services;

namespace VbMerchant.Services;
public class BasvuruService : IBasvuruService
{
private readonly IBasvuruRepository _repo;
public BasvuruService(IBasvuruRepository repo)
{
_repo = repo;
}
public async Task<List<BasvuruResponse>> GetAllAsync()
{
var list = await _repo.GetAllAsync();
return list.Select(MapToResponse).ToList();
}
public async Task<BasvuruResponse?> GetByIdAsync(int id)
{
var entity = await _repo.GetByIdAsync(id);
return entity == null ? null : MapToResponse(entity);
}
public async Task<BasvuruResponse> CreateAsync(BasvuruCreateRequest request)
{

var entity = new Basvurular
{
SirketTipiId = request.SirketTipiId,
FirmaAdi = request.FirmaAdi,
AdSoyad = request.AdSoyad,
VergiNoTckn = request.VergiNoTCKN,
VergiDairesi = request.VergiDairesi,
YetkiliTckn = request.YetkiliTCKN,
YetkiliAdSoyad = request.YetkiliAdSoyad,
EvTelefon = request.EvTelefon,
IsTelefon = request.IsTelefon,
CepTelefon = request.CepTelefon,
Email = request.Email,
Adres = request.Adres,
IlId = request.IlId,
IlceId = request.IlceId,
PostaKodu = request.PostaKodu,
WebAdres = request.WebAdres,
IsKategorisi = request.IsKategorisi,
TahminiAylikCiro = request.TahminiAylikCiro,
Enlem = (decimal?)request.Enlem,
Boylam = (decimal?)request.Boylam,
Durum = "Bekliyor", 
OlusturmaTarihi = DateTime.Now
};
var created = await _repo.CreateAsync(entity);
return MapToResponse(created);
}
public async Task UpdateDurumAsync(int id, string yeniDurum, string kullanici)
{

var entity = await _repo.GetByIdAsync(id)
?? throw new KeyNotFoundException($"Başvuru bulunamadı: {id}");
entity.Durum = yeniDurum;
entity.GuncellemeTarihi = DateTime.Now;

entity.BasvuruTarihces.Add(new BasvuruTarihce
{
Durum = yeniDurum,
Aciklama = $"Durum güncellendi: {yeniDurum}",
IslemTarihi = DateTime.Now,
Kullanici = kullanici
});
await _repo.UpdateAsync(entity);
}

private static BasvuruResponse MapToResponse(Basvurular b) => new()
{
Id = b.Id,
SirketTipi = b.SirketTipi?.TipAdi ?? string.Empty,
FirmaAdi = b.FirmaAdi,
AdSoyad = b.AdSoyad,
VergiNoTCKN = b.VergiNoTckn,
IlAdi = b.Il?.IlAdi ?? string.Empty,
IlceAdi = b.Ilce?.IlceAdi ?? string.Empty,
Email = b.Email,
Adres = b.Adres,
Durum = b.Durum,
Enlem = (double?)b.Enlem,
Boylam = (double?)b.Boylam,
OlusturmaTarihi = b.OlusturmaTarihi,
Dokumanlar = b.BasvuruDokumanlaris?.Select(d => new DokumanResponse
{
  Id = d.Id, DokumanTipi = d.DokumanTipi,
DosyaAdi = d.DosyaAdi, DosyaYolu = d.DosyaYolu,
DosyaBoyutu = d.DosyaBoyutu
}).ToList() ?? new()
};
}