using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("rol");

        builder.HasKey(r => r.RolId);
        builder.Property(r => r.RolId)
            .HasColumnName("rol_id")
            .ValueGeneratedNever();

        // NULL = rol global; con valor = rol exclusivo de esa empresa
        builder.Property(r => r.EmpresaId)
            .HasColumnName("empresa_id");

        builder.Property(r => r.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(Rol.NombreMaxLength)
            .IsRequired();

        builder.Property(r => r.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(Rol.DescripcionMaxLength);

        // Default en BD = true, pero EF siempre envía el valor
        // (si no, un "false" se ignoraría y la BD guardaría true).
        builder.Property(r => r.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .ValueGeneratedNever()
            .IsRequired();

        // Propiedad calculada en el dominio, no es columna
        builder.Ignore(r => r.EsGlobal);

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(r => r.EmpresaId)
            .HasConstraintName("fk_rol_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.EmpresaId)
            .HasDatabaseName("ix_rol_empresa_id");

        // Nombre único por empresa y también entre roles globales (empresa_id NULL):
        // en SQL Server un índice único trata los NULL como iguales, así que dos roles globales
        // con el mismo nombre chocan. HasFilter(null) quita el "WHERE empresa_id IS NOT NULL"
        // que EF agrega por defecto a los índices únicos con columnas nullable.
        // También se valida en Application para devolver un 409 con un mensaje claro.
        builder.HasIndex(r => new { r.EmpresaId, r.Nombre })
            .IsUnique()
            .HasFilter(null)
            .HasDatabaseName("uq_rol_empresa_id_nombre");
    }
}
