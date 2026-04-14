using Microsoft.EntityFrameworkCore;
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
            entity.Property(e => e.YuklemeTarihi).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Basvuru).WithMany(p => p.BasvuruDokumanlaris)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<BasvuruTarihce>(entity =>
        {
            entity.Property(e => e.IslemTarihi).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Basvuru).WithMany(p => p.BasvuruTarihces)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Basvurular>(entity =>
        {
            entity.Property(e => e.Durum).HasDefaultValue("Bekliyor");
            entity.Property(e => e.OlusturmaTarihi).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Il).WithMany(p => p.Basvurulars)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Ilce).WithMany(p => p.Basvurulars)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SirketTipi).WithMany(p => p.Basvurulars)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Ilceler>(entity =>
        {
            entity.HasOne(d => d.Il).WithMany(p => p.Ilcelers)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Kullanicilar>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OlusturmaTarihi).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Rol).HasDefaultValue("User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
