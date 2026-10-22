using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class ViajePausaConfiguration : IEntityTypeConfiguration<ViajePausa>
{
    public void Configure(EntityTypeBuilder<ViajePausa> builder)
    {
        builder.ToTable("viaje_pausa");

        builder.HasKey(p => p.ViajePausaId);
        builder.Property(p => p.ViajePausaId)
            .HasColumnName("viaje_pausa_id")
            .ValueGeneratedNever();

        builder.Property(p => p.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(p => p.ViajeId).HasColumnName("viaje_id").IsRequired();

        // La hora la manda la app (la pausa puede registrarse sin conexión)
        builder.Property(p => p.FechaInicio)
            .HasColumnName("fecha_inicio")
            .HasColumnType("timestamp")
            .IsRequired();

        builder.Property(p => p.FechaFin)
            .HasColumnName("fecha_fin")
            .HasColumnType("timestamp");

        builder.Property(p => p.Motivo)
            .HasColumnName("motivo")
            .HasMaxLength(ViajePausa.MotivoMaxLength);

        builder.Property(p => p.UbicacionLat).HasColumnName("ubicacion_lat").HasPrecision(9, 6);
        builder.Property(p => p.UbicacionLng).HasColumnName("ubicacion_lng").HasPrecision(9, 6);

        // Propiedad calculada en el dominio, no es columna
        builder.Ignore(p => p.EstaAbierta);

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(p => p.EmpresaId)
            .HasConstraintName("fk_viaje_pausa_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        // Cascade: las pausas son parte del viaje (igual que los puntos de ruta). FK compuesta tenant-safe.
        builder.HasOne<Viaje>()
            .WithMany()
            .HasForeignKey(p => new { p.EmpresaId, p.ViajeId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.ViajeId })
            .HasConstraintName("fk_viaje_pausa_viaje")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.EmpresaId)
            .HasDatabaseName("ix_viaje_pausa_empresa_id");

        builder.HasIndex(p => new { p.EmpresaId, p.ViajeId })
            .HasDatabaseName("ix_viaje_pausa_empresa_id_viaje_id");

        // Para leer las pausas de un viaje en orden y encontrar la última
        builder.HasIndex(p => new { p.ViajeId, p.FechaInicio })
            .HasDatabaseName("ix_viaje_pausa_viaje_id_fecha_inicio");
    }
}
