using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class CargaCombustibleConfiguration : IEntityTypeConfiguration<CargaCombustible>
{
    public void Configure(EntityTypeBuilder<CargaCombustible> builder)
    {
        builder.ToTable("carga_combustible");

        builder.HasKey(c => c.CargaCombustibleId);
        builder.Property(c => c.CargaCombustibleId)
            .HasColumnName("carga_combustible_id")
            .ValueGeneratedNever();

        builder.Property(c => c.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(c => c.ViajeId).HasColumnName("viaje_id").IsRequired();
        builder.Property(c => c.CentroCostoId).HasColumnName("centro_costo_id");

        builder.Property(c => c.Litros)
            .HasColumnName("litros")
            .HasPrecision(8, 2)
            .IsRequired();

        builder.Property(c => c.Monto)
            .HasColumnName("monto")
            .HasPrecision(10, 2);

        builder.Property(c => c.FotoComprobante)
            .HasColumnName("foto_comprobante")
            .HasMaxLength(CargaCombustible.FotoMaxLength)
            .IsRequired();

        builder.Property(c => c.FechaCarga)
            .HasColumnName("fecha_carga")
            .HasColumnType("timestamp")
            .IsRequired();

        builder.Property(c => c.UbicacionLat).HasColumnName("ubicacion_lat").HasPrecision(9, 6);
        builder.Property(c => c.UbicacionLng).HasColumnName("ubicacion_lng").HasPrecision(9, 6);

        // null = comprobante pendiente (genera alerta en la revisión de combustible)
        builder.Property(c => c.FechaSubidaComprobante)
            .HasColumnName("fecha_subida_comprobante")
            .HasColumnType("timestamp");

        builder.Property(c => c.Corregido)
            .HasColumnName("corregido")
            .HasDefaultValue(false)
            .ValueGeneratedNever()
            .IsRequired();

        // Hora del servidor en que llegó la carga. Reemplaza a "sincronizado" (v4).
        builder.Property(c => c.FechaRecepcion)
            .HasColumnName("fecha_recepcion")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        // Propiedad calculada en el dominio, no es columna
        builder.Ignore(c => c.PrecioPorLitro);

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .HasConstraintName("fk_carga_combustible_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict: las cargas son registros de gasto; no se borran en cascada con el viaje.
        builder.HasOne<Viaje>()
            .WithMany()
            .HasForeignKey(c => new { c.EmpresaId, c.ViajeId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.ViajeId })
            .HasConstraintName("fk_carga_combustible_viaje")
            .OnDelete(DeleteBehavior.Restrict);

        // Opcional: el centro de costos debe ser de la misma empresa de la carga
        builder.HasOne<CentroCosto>()
            .WithMany()
            .HasForeignKey(c => new { c.EmpresaId, c.CentroCostoId })
            .HasPrincipalKey(cc => new { cc.EmpresaId, cc.CentroCostoId })
            .HasConstraintName("fk_carga_combustible_centro_costo")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.EmpresaId)
            .HasDatabaseName("ix_carga_combustible_empresa_id");

        builder.HasIndex(c => new { c.EmpresaId, c.CargaCombustibleId })
            .IsUnique()
            .HasDatabaseName("uq_carga_combustible_empresa_id_carga_combustible_id");

        builder.HasIndex(c => new { c.EmpresaId, c.ViajeId })
            .HasDatabaseName("ix_carga_combustible_empresa_id_viaje_id");

        builder.HasIndex(c => new { c.EmpresaId, c.CentroCostoId })
            .HasDatabaseName("ix_carga_combustible_empresa_id_centro_costo_id");

    }
}
