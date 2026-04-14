using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VbMerchant.Data.Entities;

[Table("kullanicilar")]
[Index("Email", Name = "uq_kullanicilar_email", IsUnique = true)]
public partial class Kullanicilar
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [StringLength(100)]
    [Column("ad_soyad")]
    public string AdSoyad { get; set; } = null!;

    [StringLength(150)]
    [Unicode(false)]
    [Column("email")]
    public string Email { get; set; } = null!;

    [StringLength(500)]
    [Column("sifre_hash")]
    public string SifreHash { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    [Column("rol")]
    public string Rol { get; set; } = null!;

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("olusturma_tarihi", TypeName = "timestamp without time zone")]
    public DateTime OlusturmaTarihi { get; set; }

    [Column("son_giris_tarihi", TypeName = "timestamp without time zone")]
    public DateTime? SonGirisTarihi { get; set; }
}
