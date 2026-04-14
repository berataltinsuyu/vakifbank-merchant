using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VbMerchant.Models;

[Table("sirket_tipleri")]
[Index("TipKodu", Name = "uq_sirket_tipleri_tip_kodu", IsUnique = true)]
public partial class SirketTipleri
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [StringLength(100)]
    [Column("tip_adi")]
    public string TipAdi { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    [Column("tip_kodu")]
    public string TipKodu { get; set; } = null!;

    [InverseProperty("SirketTipi")]
    public virtual ICollection<Basvurular> Basvurulars { get; set; } = new List<Basvurular>();
}
