using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SAGWeb.Models;

namespace SAGWeb.Data;

public partial class SagrisaDbContext : DbContext
{
    public SagrisaDbContext()
    {
    }

    public SagrisaDbContext(DbContextOptions<SagrisaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<CotizacionDetalle> CotizacionDetalles { get; set; }

    public virtual DbSet<Cotizacione> Cotizaciones { get; set; }

    public virtual DbSet<SagpreciosEnLinea> SagpreciosEnLineas { get; set; }

    public virtual DbSet<SagusuariosMovil> SagusuariosMovils { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=TAKK;Database=SAGRISA_DB;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.CodCliente).HasName("PK__Clientes__DF8324D731A31185");

            entity.Property(e => e.CodCliente).ValueGeneratedOnAdd();
            entity.Property(e => e.Ciudad).HasMaxLength(100);
            entity.Property(e => e.Clase).HasMaxLength(50);
            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.Property(e => e.Lprecios)
                .HasMaxLength(50)
                .HasColumnName("LPrecios");
            entity.Property(e => e.MontoCredito).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.NomCliente).HasMaxLength(100);
            entity.Property(e => e.SaldoCredito).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TotalDeuda).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Tpago)
                .HasMaxLength(50)
                .HasColumnName("TPago");

            entity.HasOne(d => d.VendedorNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.Vendedor)
                .HasConstraintName("FK__Clientes__Vended__398D8EEE");
        });

        modelBuilder.Entity<CotizacionDetalle>(entity =>
        {
            entity.HasKey(e => e.IdDetalle).HasName("PK__Cotizaci__E43646A58B6A4851");

            entity.ToTable("CotizacionDetalle");

            entity.Property(e => e.NombreProducto).HasMaxLength(150);
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.CodCotizacionNavigation).WithMany(p => p.CotizacionDetalles)
                .HasForeignKey(d => d.CodCotizacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cotizacio__CodCo__4BAC3F29");

            entity.HasOne(d => d.CodProductoNavigation).WithMany(p => p.CotizacionDetalles)
                .HasForeignKey(d => d.CodProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cotizacio__CodPr__4CA06362");
        });

        modelBuilder.Entity<Cotizacione>(entity =>
        {
            entity.HasKey(e => e.CodCotizacion).HasName("PK__Cotizaci__79BA079E7BFF028F");

            entity.Property(e => e.FechaHora)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PrecioTotal).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.CodClienteNavigation).WithMany(p => p.Cotizaciones)
                .HasForeignKey(d => d.CodCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cotizacio__CodCl__46E78A0C");

            entity.HasOne(d => d.CodVendedorNavigation).WithMany(p => p.Cotizaciones)
                .HasForeignKey(d => d.CodVendedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cotizacio__CodVe__47DBAE45");
        });

        modelBuilder.Entity<SagpreciosEnLinea>(entity =>
        {
            entity.HasKey(e => e.CodProducto).HasName("PK__SAGPreci__0D06FDF3ACAA8CAE");

            entity.ToTable("SAGPreciosEnLinea");

            entity.Property(e => e.CodProducto).ValueGeneratedNever();
            entity.Property(e => e.Bodega).HasMaxLength(100);
            entity.Property(e => e.Clase).HasMaxLength(50);
            entity.Property(e => e.Costo).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ListaPrecio).HasMaxLength(50);
            entity.Property(e => e.NomProducto).HasMaxLength(150);
            entity.Property(e => e.Pais).HasMaxLength(50);
            entity.Property(e => e.Pbase).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Peso).HasColumnType("decimal(10, 3)");
            entity.Property(e => e.PorcentajeDesc).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PrecioSinIva)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("PrecioSinIVA");
            entity.Property(e => e.PrecioVenta).HasColumnType("decimal(12, 2)");
        });

        modelBuilder.Entity<SagusuariosMovil>(entity =>
        {
            entity.HasKey(e => e.CodVendedor).HasName("PK__SAGUsuar__25F4FC1B6412344B");

            entity.ToTable("SAGUsuariosMovil");

            entity.Property(e => e.CodVendedor).ValueGeneratedNever();
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Pais).HasMaxLength(50);
            entity.Property(e => e.Pin).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
