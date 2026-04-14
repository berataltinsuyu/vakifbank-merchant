using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VbMerchant.Models;

[Table("basvurular")]
public partial class Basvurular
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("sirket_tipi_id")]
    public int SirketTipiId { get; set; }

    [StringLength(150)]
    [Column("firma_adi")]
    public string? FirmaAdi { get; set; }

    [StringLength(100)]
    [Column("ad_soyad")]
    public string? AdSoyad { get; set; }

    [Column("vergi_no_tckn")]
    [StringLength(20)]
    [Unicode(false)]
    public string VergiNoTckn { get; set; } = null!;

    [StringLength(100)]
    [Column("vergi_dairesi")]
    public string? VergiDairesi { get; set; }

    [Column("yetkili_tckn")]
    [StringLength(11)]
    [Unicode(false)]
    public string? YetkiliTckn { get; set; }

    [StringLength(100)]
    [Column("yetkili_ad_soyad")]
    public string? YetkiliAdSoyad { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    [Column("ev_telefon")]
    public string? EvTelefon { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    [Column("is_telefon")]
    public string? IsTelefon { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    [Column("cep_telefon")]
    public string? CepTelefon { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    [Column("email")]
    public string Email { get; set; } = null!;

    [StringLength(500)]
    [Column("adres")]
    public string Adres { get; set; } = null!;

    [Column("il_id")]
    public int IlId { get; set; }

    [Column("ilce_id")]
    public int IlceId { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    [Column("posta_kodu")]
    public string? PostaKodu { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    [Column("web_adres")]
    public string? WebAdres { get; set; }

    [StringLength(150)]
    [Column("is_kategorisi")]
    public string? IsKategorisi { get; set; }

    [Column("tahmini_aylik_ciro", TypeName = "decimal(18, 2)")]
    public decimal? TahminiAylikCiro { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    [Column("durum")]
    public string Durum { get; set; } = null!;

    [Column("enlem", TypeName = "decimal(10, 7)")]
    public decimal? Enlem { get; set; }

    [Column("boylam", TypeName = "decimal(10, 7)")]
    public decimal? Boylam { get; set; }

    [Column("olusturma_tarihi", TypeName = "timestamp without time zone")]
    public DateTime OlusturmaTarihi { get; set; }

    [Column("guncelleme_tarihi", TypeName = "timestamp without time zone")]
    public DateTime? GuncellemeTarihi { get; set; }

    [InverseProperty("Basvuru")]
    public virtual ICollection<BasvuruDokumanlari> BasvuruDokumanlaris { get; set; } = new List<BasvuruDokumanlari>();

    [InverseProperty("Basvuru")]
    public virtual ICollection<BasvuruTarihce> BasvuruTarihces { get; set; } = new List<BasvuruTarihce>();

    [ForeignKey("IlId")]
    [InverseProperty("Basvurulars")]
    public virtual Iller Il { get; set; } = null!;

    [ForeignKey("IlceId")]
    [InverseProperty("Basvurulars")]
    public virtual Ilceler Ilce { get; set; } = null!;

    [ForeignKey("SirketTipiId")]
    [InverseProperty("Basvurulars")]
    public virtual SirketTipleri SirketTipi { get; set; } = null!;
}
