using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VbMerchant.Models;

[Table("BasvuruDokumanlari")]
public partial class BasvuruDokumanlari
{
    [Key]
    public int Id { get; set; }

    public int BasvuruId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string DokumanTipi { get; set; } = null!;

    [StringLength(260)]
    public string DosyaAdi { get; set; } = null!;

    [StringLength(500)]
    public string DosyaYolu { get; set; } = null!;

    public long DosyaBoyutu { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime YuklemeTarihi { get; set; }

    [ForeignKey("BasvuruId")]
    [InverseProperty("BasvuruDokumanlaris")]
    public virtual Basvurular Basvuru { get; set; } = null!;
}
