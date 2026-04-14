using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VbMerchant.Models;

[Table("basvuru_dokumanlari")]
public partial class BasvuruDokumanlari
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("basvuru_id")]
    public int BasvuruId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    [Column("dokuman_tipi")]
    public string DokumanTipi { get; set; } = null!;

    [StringLength(260)]
    [Column("dosya_adi")]
    public string DosyaAdi { get; set; } = null!;

    [StringLength(500)]
    [Column("dosya_yolu")]
    public string DosyaYolu { get; set; } = null!;

    [Column("dosya_boyutu")]
    public long DosyaBoyutu { get; set; }

    [Column("yukleme_tarihi", TypeName = "timestamp without time zone")]
    public DateTime YuklemeTarihi { get; set; }

    [ForeignKey("BasvuruId")]
    [InverseProperty("BasvuruDokumanlaris")]
    public virtual Basvurular Basvuru { get; set; } = null!;
}
