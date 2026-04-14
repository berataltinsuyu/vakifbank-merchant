using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VbMerchant.Models;

[Table("iller")]
[Index("IlKodu", Name = "uq_iller_il_kodu", IsUnique = true)]
public partial class Iller
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    [Column("il_kodu")]
    public string IlKodu { get; set; } = null!;

    [StringLength(50)]
    [Column("il_adi")]
    public string IlAdi { get; set; } = null!;

    [InverseProperty("Il")]
    public virtual ICollection<Basvurular> Basvurulars { get; set; } = new List<Basvurular>();

    [InverseProperty("Il")]
    public virtual ICollection<Ilceler> Ilcelers { get; set; } = new List<Ilceler>();
}
