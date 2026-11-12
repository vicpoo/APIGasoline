using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class CorporativoConfiguration : IEntityTypeConfiguration<Corporativo>
{
    public void Configure(EntityTypeBuilder<Corporativo> builder)
    {
        builder.ToTable("corporativo");

        builder.HasKey(c => c.CorporativoId);
        builder.Property(c => c.CorporativoId)
            .HasColumnName("corporativo_id")
            .ValueGeneratedNever();

        builder.Property(c => c.EmpresaId)
            .HasColumnName("empresa_id")
            .IsRequired();

        builder.Property(c => c.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(Corporativo.NombreMaxLength)
            .IsRequired();

        // Default en BD = true, pero EF siempre envía el valor
        // (si no, un "false" se ignoraría y la BD guardaría true).
        builder.Property(c => c.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .ValueGeneratedNever()
            .IsRequired();

        // La BD la llena al insertar; EF la lee de vuelta y nunca la envía.
        builder.Property(c => c.FechaRegistro)
            .HasColumnName("fecha_registro")
            .HasColumnType(TiposSql.FechaHora)
            .HasDefaultValueSql(TiposSql.Ahora)
            .ValueGeneratedOnAdd();

        // Restrict: la empresa usa soft delete, nunca se borra físicamente con corporativos.
        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .HasConstraintName("fk_corporativo_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.EmpresaId, c.Nombre })
            .IsUnique()
            .HasDatabaseName("uq_corporativo_empresa_id_nombre");

        // Llave alterna: la referencian corporativo_division y centro_costo con FK compuesta (tenant-safe)
        builder.HasAlternateKey(c => new { c.EmpresaId, c.CorporativoId })
            .HasName("uq_corporativo_empresa_id_corporativo_id");
    }
}
