using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VbMerchant.Models;

[Table("Basvurular")]
public partial class Basvurular
{
    [Key]
    public int Id { get; set; }

    public int SirketTipiId { get; set; }

    [StringLength(150)]
    public string? FirmaAdi { get; set; }

    [StringLength(100)]
    public string? AdSoyad { get; set; }

    [Column("VergiNoTCKN")]
    [StringLength(20)]
    [Unicode(false)]
    public string VergiNoTckn { get; set; } = null!;

    [StringLength(100)]
    public string? VergiDairesi { get; set; }

    [Column("YetkiliTCKN")]
    [StringLength(11)]
    [Unicode(false)]
    public string? YetkiliTckn { get; set; }

    [StringLength(100)]
    public string? YetkiliAdSoyad { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? EvTelefon { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? IsTelefon { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? CepTelefon { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [StringLength(500)]
    public string Adres { get; set; } = null!;

    public int IlId { get; set; }

    public int IlceId { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? PostaKodu { get; set; }

    [StringLength(200)]
    [Unicode(false)]
    public string? WebAdres { get; set; }

    [StringLength(150)]
    public string? IsKategorisi { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TahminiAylikCiro { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Durum { get; set; } = null!;

    [Column(TypeName = "decimal(10, 7)")]
    public decimal? Enlem { get; set; }

    [Column(TypeName = "decimal(10, 7)")]
    public decimal? Boylam { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime OlusturmaTarihi { get; set; }

    [Column(TypeName = "datetime")]
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
