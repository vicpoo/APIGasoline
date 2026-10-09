using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class DivisionConfiguration : IEntityTypeConfiguration<Division>
{
    public void Configure(EntityTypeBuilder<Division> builder)
    {
        builder.ToTable("division");

        builder.HasKey(d => d.DivisionId);
        builder.Property(d => d.DivisionId)
            .HasColumnName("division_id")
            .ValueGeneratedNever();

        builder.Property(d => d.CorporativoId)
            .HasColumnName("corporativo_id")
            .IsRequired();

        builder.Property(d => d.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(Division.NombreMaxLength)
            .IsRequired();

        // Default en BD = true, pero EF siempre envía el valor
        // (si no, un "false" se ignoraría y la BD guardaría true).
        builder.Property(d => d.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .ValueGeneratedNever()
            .IsRequired();

        // La BD la llena al insertar; EF la lee de vuelta y nunca la envía.
        builder.Property(d => d.FechaRegistro)
            .HasColumnName("fecha_registro")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        // Restrict: no se puede borrar un corporativo que todavía tiene divisiones.
        builder.HasOne<Corporativo>()
            .WithMany()
            .HasForeignKey(d => d.CorporativoId)
            .HasConstraintName("fk_division_corporativo")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.CorporativoId)
            .HasDatabaseName("ix_division_corporativo_id");

        // Llave alterna: es la que referencian las FK compuestas (tenant-safe)
        builder.HasAlternateKey(d => new { d.CorporativoId, d.DivisionId })
            .HasName("uq_division_corporativo_id_division_id");
    }
}
