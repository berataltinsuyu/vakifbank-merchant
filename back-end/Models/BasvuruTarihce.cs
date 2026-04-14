using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VbMerchant.Models;

[Table("basvuru_tarihce")]
public partial class BasvuruTarihce
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("basvuru_id")]
    public int BasvuruId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    [Column("durum")]
    public string Durum { get; set; } = null!;

    [StringLength(500)]
    [Column("aciklama")]
    public string? Aciklama { get; set; }

    [Column("islem_tarihi", TypeName = "timestamp without time zone")]
    public DateTime IslemTarihi { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    [Column("kullanici")]
    public string Kullanici { get; set; } = null!;

    [ForeignKey("BasvuruId")]
    [InverseProperty("BasvuruTarihces")]
    public virtual Basvurular Basvuru { get; set; } = null!;
}
