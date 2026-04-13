using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using VbMerchant.Data;
using VbMerchant.Data.Entities;
using VbMerchant.Models;

namespace VbMerchant.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BasvuruDokumanlari> BasvuruDokumanlaris { get; set; }

    public virtual DbSet<BasvuruTarihce> BasvuruTarihces { get; set; }

    public virtual DbSet<Basvurular> Basvurulars { get; set; }

    public virtual DbSet<Ilceler> Ilcelers { get; set; }

    public virtual DbSet<Iller> Illers { get; set; }

    public virtual DbSet<Kullanicilar> Kullanicilars { get; set; }

    public virtual DbSet<SirketTipleri> SirketTipleris { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<BasvuruDokumanlari>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BasvuruD__3214EC078443DA1E");

            entity.Property(e => e.YuklemeTarihi).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Basvuru).WithMany(p => p.BasvuruDokumanlaris)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Dokuman_Basvuru");
        });

        modelBuilder.Entity<BasvuruTarihce>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BasvuruT__3214EC07BC4014CB");

            entity.Property(e => e.IslemTarihi).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Basvuru).WithMany(p => p.BasvuruTarihces)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tarihce_Basvuru");
        });

        modelBuilder.Entity<Basvurular>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Basvurul__3214EC07779917A8");

            entity.Property(e => e.Durum).HasDefaultValue("Bekliyor");
            entity.Property(e => e.OlusturmaTarihi).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Il).WithMany(p => p.Basvurulars)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Basvurular_Il");

            entity.HasOne(d => d.Ilce).WithMany(p => p.Basvurulars)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Basvurular_Ilce");

            entity.HasOne(d => d.SirketTipi).WithMany(p => p.Basvurulars)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Basvurular_SirketTipi");
        });


        modelBuilder.Entity<Ilceler>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ilceler__3214EC073BAE4E53");

            entity.HasOne(d => d.Il).WithMany(p => p.Ilcelers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ilceler_Iller");
        });

        modelBuilder.Entity<Iller>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Iller__3214EC07EAA4E4E1");
        });


        modelBuilder.Entity<Kullanicilar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Kullanic__3214EC07987385FC");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OlusturmaTarihi).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Rol).HasDefaultValue("User");
        });


        modelBuilder.Entity<SirketTipleri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SirketTi__3214EC076D6A41C0");
        });



        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
