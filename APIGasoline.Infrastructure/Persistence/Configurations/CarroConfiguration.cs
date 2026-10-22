using APIGasoline.Domain.Entities;
using APIGasoline.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class CarroConfiguration : IEntityTypeConfiguration<Carro>
{
    public void Configure(EntityTypeBuilder<Carro> builder)
    {
        builder.ToTable("carro");

        builder.HasKey(c => c.CarroId);
        builder.Property(c => c.CarroId)
            .HasColumnName("carro_id")
            .ValueGeneratedNever();

        builder.Property(c => c.EmpresaId)
            .HasColumnName("empresa_id")
            .IsRequired();

        builder.Property(c => c.Marca)
            .HasColumnName("marca")
            .HasMaxLength(Carro.MarcaMaxLength)
            .IsRequired();

        builder.Property(c => c.Modelo)
            .HasColumnName("modelo")
            .HasMaxLength(Carro.ModeloMaxLength)
            .IsRequired();

        builder.Property(c => c.Anio)
            .HasColumnName("anio");

        builder.Property(c => c.Color)
            .HasColumnName("color")
            .HasMaxLength(Carro.ColorMaxLength);

        builder.Property(c => c.Placas)
            .HasColumnName("placas")
            .HasMaxLength(Carro.PlacasMaxLength)
            .IsRequired();

        builder.Property(c => c.FotoVehiculo)
            .HasColumnName("foto_vehiculo")
            .HasMaxLength(Carro.FotoMaxLength);

        builder.Property(c => c.CapacidadTanque)
            .HasColumnName("capacidad_tanque")
            .HasPrecision(6, 2);

        builder.Property(c => c.KilometrajeActual)
            .HasColumnName("kilometraje_actual")
            .HasPrecision(10, 2)
            .HasDefaultValue(0m)
            .ValueGeneratedNever()
            .IsRequired();

        // enum('disponible','ocupado','en_mantenimiento') en MySQL
        builder.Property(c => c.Estado)
            .HasColumnName("estado")
            .HasColumnType(EstadoCarroConverter.ColumnType)
            .HasConversion(new EstadoCarroConverter())
            .HasDefaultValueSql("'disponible'")
            .ValueGeneratedNever()
            // Concurrencia optimista: dos cambios de estado simultáneos sobre el mismo carro
            // (ej. dos aprobaciones) no pueden ganar los dos; el segundo recibe 409.
            .IsConcurrencyToken()
            .IsRequired();

        // Baja del carro (HU-AST-03)
        builder.Property(c => c.DadoDeBaja)
            .HasColumnName("dado_de_baja")
            .HasDefaultValue(false)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(c => c.FechaBaja)
            .HasColumnName("fecha_baja")
            .HasColumnType("timestamp");

        builder.Property(c => c.MotivoBaja)
            .HasColumnName("motivo_baja")
            .HasMaxLength(Carro.MotivoBajaMaxLength);

        // Propiedad calculada en el dominio, no es columna
        builder.Ignore(c => c.EstaDisponible);

        // La BD la llena al insertar; EF la lee de vuelta y nunca la envía.
        builder.Property(c => c.FechaAlta)
            .HasColumnName("fecha_alta")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        // Restrict: la empresa usa soft delete, nunca se borra físicamente con carros.
        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .HasConstraintName("fk_carro_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.Placas)
            .IsUnique()
            .HasDatabaseName("uq_carro_placas");

        builder.HasIndex(c => c.EmpresaId)
            .HasDatabaseName("ix_carro_empresa_id");

        // Llave alterna: es la que referencian las FK compuestas (tenant-safe)
        builder.HasAlternateKey(c => new { c.EmpresaId, c.CarroId })
            .HasName("uq_carro_empresa_id_carro_id");
    }
}
