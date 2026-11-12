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

        builder.Property(d => d.EmpresaId)
            .HasColumnName("empresa_id")
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
            .HasColumnType(TiposSql.FechaHora)
            .HasDefaultValueSql(TiposSql.Ahora)
            .ValueGeneratedOnAdd();

        // Restrict: la empresa usa soft delete, nunca se borra físicamente con divisiones.
        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(d => d.EmpresaId)
            .HasConstraintName("fk_division_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new { d.EmpresaId, d.Nombre })
            .IsUnique()
            .HasDatabaseName("uq_division_empresa_id_nombre");

        // Llave alterna: la referencian corporativo_division y centro_costo con FK compuesta (tenant-safe)
        builder.HasAlternateKey(d => new { d.EmpresaId, d.DivisionId })
            .HasName("uq_division_empresa_id_division_id");
    }
}
