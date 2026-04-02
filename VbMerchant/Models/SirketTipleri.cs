using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VbMerchant.Models;

[Table("SirketTipleri")]
[Index("TipKodu", Name = "UQ__SirketTi__E098772A81EA07D4", IsUnique = true)]
public partial class SirketTipleri
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string TipAdi { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string TipKodu { get; set; } = null!;

    [InverseProperty("SirketTipi")]
    public virtual ICollection<Basvurular> Basvurulars { get; set; } = new List<Basvurular>();
}
