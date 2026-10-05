using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class PuntoRutaConfiguration : IEntityTypeConfiguration<PuntoRuta>
{
    public void Configure(EntityTypeBuilder<PuntoRuta> builder)
    {
        builder.ToTable("punto_ruta");

        builder.HasKey(p => p.PuntoRutaId);
        builder.Property(p => p.PuntoRutaId)
            .HasColumnName("punto_ruta_id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(p => p.ViajeId).HasColumnName("viaje_id").IsRequired();

        builder.Property(p => p.Latitud)
            .HasColumnName("latitud")
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(p => p.Longitud)
            .HasColumnName("longitud")
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(p => p.TimestampCaptura)
            .HasColumnName("timestamp_captura")
            .HasColumnType("timestamp")
            .IsRequired();

        builder.Property(p => p.Sincronizado)
            .HasColumnName("sincronizado")
            .HasDefaultValue(false)
            .ValueGeneratedNever()
            .IsRequired();

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(p => p.EmpresaId)
            .HasConstraintName("fk_punto_ruta_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        // Cascade: los puntos son parte del viaje; si se borra el viaje, se borra su ruta.
        builder.HasOne<Viaje>()
            .WithMany()
            .HasForeignKey(p => new { p.EmpresaId, p.ViajeId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.ViajeId })
            .HasConstraintName("fk_punto_ruta_viaje")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.EmpresaId)
            .HasDatabaseName("ix_punto_ruta_empresa_id");

        builder.HasIndex(p => new { p.EmpresaId, p.PuntoRutaId })
            .IsUnique()
            .HasDatabaseName("uq_punto_ruta_empresa_id_punto_ruta_id");

        builder.HasIndex(p => new { p.EmpresaId, p.ViajeId })
            .HasDatabaseName("ix_punto_ruta_empresa_id_viaje_id");

        // Para la llave foránea y para leer la ruta ordenada por tiempo
        builder.HasIndex(p => new { p.ViajeId, p.TimestampCaptura })
            .HasDatabaseName("ix_punto_ruta_viaje_id_timestamp_captura");
    }
}
