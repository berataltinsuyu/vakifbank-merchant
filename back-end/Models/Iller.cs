using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VbMerchant.Models;

[Table("Iller")]
[Index("IlKodu", Name = "UQ__Iller__64422E3524A7B85D", IsUnique = true)]
public partial class Iller
{
    [Key]
    public int Id { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string IlKodu { get; set; } = null!;

    [StringLength(50)]
    public string IlAdi { get; set; } = null!;

    [InverseProperty("Il")]
    public virtual ICollection<Basvurular> Basvurulars { get; set; } = new List<Basvurular>();

    [InverseProperty("Il")]
    public virtual ICollection<Ilceler> Ilcelers { get; set; } = new List<Ilceler>();
}
