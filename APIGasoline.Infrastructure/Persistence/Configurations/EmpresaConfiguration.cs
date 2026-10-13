using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresa");

        builder.HasKey(e => e.EmpresaId);
        builder.Property(e => e.EmpresaId)
            .HasColumnName("empresa_id")
            .ValueGeneratedNever();

        builder.Property(e => e.CorporativoId)
            .HasColumnName("corporativo_id");

        builder.Property(e => e.DivisionId)
            .HasColumnName("division_id");

        builder.Property(e => e.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(Empresa.NombreMaxLength)
            .IsRequired();

        // Soft delete. v4: la columna se renombra de "delete" (palabra reservada) a "eliminado".
        builder.Property(e => e.Eliminado)
            .HasColumnName("eliminado")
            .HasDefaultValue(false)
            .ValueGeneratedNever()
            .IsRequired();

        // Empresa activa o inactiva (HU-ADM-01)
        builder.Property(e => e.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .ValueGeneratedNever()
            .IsRequired();

        // La BD la llena al insertar; EF la lee de vuelta y nunca la envía.
        builder.Property(e => e.FechaRegistro)
            .HasColumnName("fecha_registro")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        // Filtro global: todas las consultas ignoran las empresas eliminadas.
        builder.HasQueryFilter(e => !e.Eliminado);

        // Restrict: no se puede borrar un corporativo o una división que tenga empresas.
        builder.HasOne<Corporativo>()
            .WithMany()
            .HasForeignKey(e => e.CorporativoId)
            .HasConstraintName("fk_empresa_corporativo")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Division>()
            .WithMany()
            .HasForeignKey(e => new { e.CorporativoId, e.DivisionId })
            .HasPrincipalKey(d => new { d.CorporativoId, d.DivisionId })
            .HasConstraintName("fk_empresa_division")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CorporativoId)
            .HasDatabaseName("ix_empresa_corporativo_id");

        builder.HasIndex(e => e.DivisionId)
            .HasDatabaseName("ix_empresa_division_id");

        // Para la FK compuesta hacia division
        builder.HasIndex(e => new { e.CorporativoId, e.DivisionId })
            .HasDatabaseName("ix_empresa_corporativo_id_division_id");

        builder.HasIndex(e => new { e.CorporativoId, e.EmpresaId })
            .IsUnique()
            .HasDatabaseName("uq_empresa_corporativo_id_empresa_id");
    }
}
