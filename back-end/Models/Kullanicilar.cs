using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VbMerchant.Data.Entities;

[Table("Kullanicilar")]
[Index("Email", Name = "UQ__Kullanic__A9D105347B1CAD4F", IsUnique = true)]
public partial class Kullanicilar
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string AdSoyad { get; set; } = null!;

    [StringLength(150)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [StringLength(500)]
    public string SifreHash { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Rol { get; set; } = null!;

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime OlusturmaTarihi { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SonGirisTarihi { get; set; }
}
