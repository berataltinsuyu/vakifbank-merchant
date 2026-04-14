using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VbMerchant.Models;

[Table("ilceler")]
public partial class Ilceler
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("il_id")]
    public int IlId { get; set; }

    [StringLength(80)]
    [Column("ilce_adi")]
    public string IlceAdi { get; set; } = null!;

    [InverseProperty("Ilce")]
    public virtual ICollection<Basvurular> Basvurulars { get; set; } = new List<Basvurular>();

    [ForeignKey("IlId")]
    [InverseProperty("Ilcelers")]
    public virtual Iller Il { get; set; } = null!;
}
