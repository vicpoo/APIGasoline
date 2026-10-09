using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class RutaProcesadaConfiguration : IEntityTypeConfiguration<RutaProcesada>
{
    public void Configure(EntityTypeBuilder<RutaProcesada> builder)
    {
        builder.ToTable("ruta_procesada");

        builder.HasKey(r => r.RutaProcesadaId);
        builder.Property(r => r.RutaProcesadaId)
            .HasColumnName("ruta_procesada_id")
            .ValueGeneratedNever();

        builder.Property(r => r.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(r => r.ViajeId).HasColumnName("viaje_id").IsRequired();

        // --- Limpieza ---
        builder.Property(r => r.PuntosRecibidos)
            .HasColumnName("puntos_recibidos")
            .HasDefaultValue(0)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(r => r.PuntosDescartados)
            .HasColumnName("puntos_descartados")
            .HasDefaultValue(0)
            .ValueGeneratedNever()
            .IsRequired();

        // --- Distancia y tiempos según el GPS ---
        builder.Property(r => r.DistanciaGpsKm)
            .HasColumnName("distancia_gps_km")
            .HasPrecision(10, 3)
            .HasDefaultValue(0m)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(r => r.TiempoMovimientoSeg)
            .HasColumnName("tiempo_movimiento_seg")
            .HasDefaultValue(0)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(r => r.TiempoDetenidoSeg)
            .HasColumnName("tiempo_detenido_seg")
            .HasDefaultValue(0)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(r => r.VelocidadPromedioKmh).HasColumnName("velocidad_promedio_kmh").HasPrecision(6, 2);
        builder.Property(r => r.VelocidadMaximaKmh).HasColumnName("velocidad_maxima_kmh").HasPrecision(6, 2);

        // --- GPS contra odómetro ---
        builder.Property(r => r.DistanciaOdometroKm).HasColumnName("distancia_odometro_km").HasPrecision(10, 2);
        builder.Property(r => r.DiferenciaKm).HasColumnName("diferencia_km").HasPrecision(10, 2);
        builder.Property(r => r.DiferenciaPorcentaje).HasColumnName("diferencia_porcentaje").HasPrecision(7, 2);

        builder.Property(r => r.AlertaKilometraje)
            .HasColumnName("alerta_kilometraje")
            .HasDefaultValue(false)
            .ValueGeneratedNever()
            .IsRequired();

        // --- Combustible ---
        builder.Property(r => r.LitrosCargados)
            .HasColumnName("litros_cargados")
            .HasPrecision(8, 2)
            .HasDefaultValue(0m)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(r => r.RendimientoKmPorLitro).HasColumnName("rendimiento_km_por_litro").HasPrecision(6, 2);

        // --- Mapa ---
        builder.Property(r => r.Polyline)
            .HasColumnName("polyline")
            .HasColumnType("text");

        // --- Control de versiones del algoritmo ---
        builder.Property(r => r.VersionAlgoritmo)
            .HasColumnName("version_algoritmo")
            .HasMaxLength(20)
            .IsRequired();

        // El dominio siempre la asigna (también al recalcular), por eso EF sí la envía.
        builder.Property(r => r.FechaProceso)
            .HasColumnName("fecha_proceso")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedNever()
            .IsRequired();

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(r => r.EmpresaId)
            .HasConstraintName("fk_ruta_procesada_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        // Cascade: el resumen es derivado del viaje; si se borra el viaje, se borra su resumen.
        builder.HasOne<Viaje>()
            .WithMany()
            .HasForeignKey(r => new { r.EmpresaId, r.ViajeId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.ViajeId })
            .HasConstraintName("fk_ruta_procesada_viaje")
            .OnDelete(DeleteBehavior.Cascade);

        // 1:1 con viaje: al recalcular se actualiza la misma fila.
        builder.HasIndex(r => r.ViajeId)
            .IsUnique()
            .HasDatabaseName("uq_ruta_procesada_viaje_id");

        builder.HasIndex(r => r.EmpresaId)
            .HasDatabaseName("ix_ruta_procesada_empresa_id");

        builder.HasIndex(r => new { r.EmpresaId, r.RutaProcesadaId })
            .IsUnique()
            .HasDatabaseName("uq_ruta_procesada_empresa_id_ruta_procesada_id");

        builder.HasIndex(r => new { r.EmpresaId, r.ViajeId })
            .HasDatabaseName("ix_ruta_procesada_empresa_id_viaje_id");

        // Para listar rápido los viajes con alerta de kilometraje por empresa.
        builder.HasIndex(r => new { r.EmpresaId, r.AlertaKilometraje })
            .HasDatabaseName("ix_ruta_procesada_empresa_id_alerta_kilometraje");
    }
}
