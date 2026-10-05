using APIGasoline.Domain.Entities;
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
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(Privilegio.NombreMaxLength)
            .IsRequired();

        builder.HasIndex(p => p.Nombre)
            .IsUnique()
            .HasDatabaseName("uq_privilegio_nombre");

        builder.Property(p => p.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(Privilegio.DescripcionMaxLength);

        // Default en BD = true, pero EF siempre envía el valor
        // (si no, un "false" se ignoraría y la BD guardaría true).
        builder.Property(p => p.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .ValueGeneratedNever()
            .IsRequired();
    }
}
