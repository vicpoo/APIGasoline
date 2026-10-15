using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class PrivilegioConfiguration : IEntityTypeConfiguration<Privilegio>
{
    public void Configure(EntityTypeBuilder<Privilegio> builder)
    {
        builder.ToTable("privilegio");

        builder.HasKey(p => p.PrivilegioId);
        builder.Property(p => p.PrivilegioId)
            .HasColumnName("privilegio_id")
            .ValueGeneratedNever();

        // Código técnico inmutable (modulo.accion). Es lo que usan la API y el front.
        builder.Property(p => p.Clave)
            .HasColumnName("clave")
            .HasMaxLength(Privilegio.ClaveMaxLength)
            .IsRequired();

        builder.HasIndex(p => p.Clave)
            .IsUnique()
            .HasDatabaseName("uq_privilegio_clave");

        builder.Property(p => p.Modulo)
            .HasColumnName("modulo")
            .HasMaxLength(Privilegio.ModuloMaxLength)
            .IsRequired();

        builder.HasIndex(p => p.Modulo)
            .HasDatabaseName("ix_privilegio_modulo");

        // Solo texto visible: ya no es único (v5)
        builder.Property(p => p.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(Privilegio.NombreMaxLength)
            .IsRequired();

        builder.Property(p => p.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(Privilegio.DescripcionMaxLength);

        // enum('plataforma','empresa'). Default en BD = 'empresa', pero EF siempre envía el valor
        // (Plataforma es el valor 0 del enum y, si no, EF lo omitiría y la BD guardaría 'empresa').
        builder.Property(p => p.Ambito)
            .HasColumnName("ambito")
            .HasColumnType(SnakeCaseEnumConverter<AmbitoPrivilegio>.ColumnType)
            .HasConversion(new SnakeCaseEnumConverter<AmbitoPrivilegio>())
            .HasDefaultValueSql("'empresa'")
            .ValueGeneratedNever()
            .IsRequired();

        // Default en BD = true, pero EF siempre envía el valor
        // (si no, un "false" se ignoraría y la BD guardaría true).
        builder.Property(p => p.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .ValueGeneratedNever()
            .IsRequired();
    }
}
