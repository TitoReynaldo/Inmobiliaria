using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Inmobiliaria.API.Models;

public partial class InmobiliariaContext : DbContext
{
    public InmobiliariaContext()
    {
    }

    public InmobiliariaContext(DbContextOptions<InmobiliariaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }
    public virtual DbSet<Configfinanciera> Configfinancieras { get; set; }
    public virtual DbSet<Detallecronograma> Detallecronogramas { get; set; }
    public virtual DbSet<Maestrobono> Maestrobonos { get; set; }
    public virtual DbSet<Propiedade> Propiedades { get; set; }
    public virtual DbSet<Simulacione> Simulaciones { get; set; }
    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.ClienteId).HasName("PRIMARY");

            entity.ToTable("clientes");

            entity.HasIndex(e => e.Dni, "DNI").IsUnique();

            entity.HasIndex(e => e.UsuarioId, "UsuarioID");

            entity.Property(e => e.ClienteId).HasColumnName("ClienteID");
            entity.Property(e => e.Apellidos).HasMaxLength(100);
            entity.Property(e => e.Dni)
                .HasMaxLength(8)
                .HasColumnName("DNI");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Nombres).HasMaxLength(100);
            entity.Property(e => e.SueldoMensual).HasPrecision(30, 15);
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("clientes_ibfk_1");
        });

        modelBuilder.Entity<Configfinanciera>(entity =>
        {
            entity.HasKey(e => e.ConfigId).HasName("PRIMARY");

            entity.ToTable("configfinanciera");

            entity.Property(e => e.ConfigId).HasColumnName("ConfigID");
            entity.Property(e => e.NombreBanco).HasMaxLength(50);
            entity.Property(e => e.PlazoMaximoMeses).HasDefaultValueSql("'240'");
            entity.Property(e => e.PlazoMinimoMeses).HasDefaultValueSql("'60'");
            entity.Property(e => e.PorcentajeDesgravamen).HasPrecision(30, 15);
            entity.Property(e => e.PorcentajeSeguroInmueble).HasPrecision(30, 15);
            entity.Property(e => e.TasaInteres).HasPrecision(30, 15);
            entity.Property(e => e.TipoTasa)
                .HasDefaultValueSql("'Efectiva'")
                .HasColumnType("enum('Nominal','Efectiva')");
        });

        modelBuilder.Entity<Detallecronograma>(entity =>
        {
            entity.HasKey(e => e.DetalleId).HasName("PRIMARY");

            entity.ToTable("detallecronograma");

            entity.HasIndex(e => e.SimulacionId, "SimulacionID");

            entity.Property(e => e.DetalleId).HasColumnName("DetalleID");
            entity.Property(e => e.Amortizacion).HasColumnType("decimal(30, 15)");
            entity.Property(e => e.Cuota).HasColumnType("decimal(30, 15)");
            entity.Property(e => e.CuotaTotal).HasColumnType("decimal(30, 15)");
            entity.Property(e => e.Interes).HasColumnType("decimal(30, 15)");
            entity.Property(e => e.SaldoFinal).HasColumnType("decimal(30, 15)");
            entity.Property(e => e.SaldoInicial).HasColumnType("decimal(30, 15)");
            entity.Property(e => e.SeguroDesgravamen).HasColumnType("decimal(30, 15)");
            entity.Property(e => e.SeguroInmueble).HasColumnType("decimal(30, 15)");
            entity.Property(e => e.SimulacionId).HasColumnName("SimulacionID");
            entity.Property(e => e.TasaEfectivaPeriodo).HasColumnType("decimal(30, 15)");

            entity.HasOne(d => d.Simulacion).WithMany(p => p.Detallecronogramas)
                .HasForeignKey(d => d.SimulacionId)
                .HasConstraintName("detallecronograma_ibfk_1");
        });

        modelBuilder.Entity<Maestrobono>(entity =>
        {
            entity.HasKey(e => e.BonoId).HasName("PRIMARY");

            entity.ToTable("maestrobonos");

            entity.Property(e => e.BonoId).HasColumnName("BonoID");
            entity.Property(e => e.MontoBono).HasColumnType("decimal(30, 15)");
            entity.Property(e => e.PrecioViviendaDesde).HasColumnType("decimal(30, 15)");
            entity.Property(e => e.PrecioViviendaHasta).HasColumnType("decimal(30, 15)");
        });

        modelBuilder.Entity<Propiedade>(entity =>
        {
            entity.HasKey(e => e.PropiedadId).HasName("PRIMARY");

            entity.ToTable("propiedades");

            entity.Property(e => e.PropiedadId).HasColumnName("PropiedadID");
            entity.Property(e => e.Direccion).HasMaxLength(255);
            entity.Property(e => e.EsBonoVerde).HasDefaultValueSql("'0'");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'Disponible'")
                .HasColumnType("enum('Disponible','Vendido')");
            entity.Property(e => e.PrecioVenta).HasColumnType("decimal(30, 15)");
        });

        modelBuilder.Entity<Simulacione>(entity =>
        {
            entity.HasKey(e => e.SimulacionId).HasName("PRIMARY");

            entity.ToTable("simulaciones");

            entity.HasIndex(e => e.ClienteId, "ClienteID");

            entity.HasIndex(e => e.ConfigId, "ConfigID");

            entity.HasIndex(e => e.PropiedadId, "PropiedadID");

            entity.Property(e => e.SimulacionId).HasColumnName("SimulacionID");
            entity.Property(e => e.ClienteId).HasColumnName("ClienteID");
            entity.Property(e => e.ConfigId).HasColumnName("ConfigID");
            entity.Property(e => e.CuotaInicial).HasColumnType("decimal(30, 15)");
            entity.Property(e => e.FechaSimulacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");//TRAS
            entity.Property(e => e.MesesGracia).HasDefaultValueSql("'0'");
            entity.Property(e => e.MontoPrestamo).HasColumnType("decimal(30, 15)");
            entity.Property(e => e.PrecioVenta).HasColumnType("decimal(30, 15)");
            entity.Property(e => e.PropiedadId).HasColumnName("PropiedadID");
            entity.Property(e => e.TasaEfectivaAnual).HasColumnType("decimal(30, 15)");
            entity.Property(e => e.Tcea)
                .HasColumnType("decimal(30, 15)")
                .HasColumnName("TCEA");
            entity.Property(e => e.TipoGracia)
                .HasDefaultValueSql("'Sin Gracia'")
                .HasColumnType("enum('Sin Gracia','Parcial','Total')");
            entity.Property(e => e.Tir)
                .HasColumnType("decimal(30, 15)")
                .HasColumnName("TIR");
            entity.Property(e => e.Van)
                .HasColumnType("decimal(30, 15)")
                .HasColumnName("VAN");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Simulaciones)
                .HasForeignKey(d => d.ClienteId)
                .HasConstraintName("simulaciones_ibfk_1");

            entity.HasOne(d => d.Config).WithMany(p => p.Simulaciones)
                .HasForeignKey(d => d.ConfigId)
                .HasConstraintName("simulaciones_ibfk_3");

            entity.HasOne(d => d.Propiedad).WithMany(p => p.Simulaciones)
                .HasForeignKey(d => d.PropiedadId)
                .HasConstraintName("simulaciones_ibfk_2");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PRIMARY");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Username, "Username").IsUnique();

            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Rol)
                .HasDefaultValueSql("'Cliente'")
                .HasColumnType("enum('Admin','Cliente')");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}