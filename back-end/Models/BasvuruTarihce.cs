using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VbMerchant.Models;

[Table("BasvuruTarihce")]
public partial class BasvuruTarihce
{
    [Key]
    public int Id { get; set; }

    public int BasvuruId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Durum { get; set; } = null!;

    [StringLength(500)]
    public string? Aciklama { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime IslemTarihi { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Kullanici { get; set; } = null!;

    [ForeignKey("BasvuruId")]
    [InverseProperty("BasvuruTarihces")]
    public virtual Basvurular Basvuru { get; set; } = null!;
}
