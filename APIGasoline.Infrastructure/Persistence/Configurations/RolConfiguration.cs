using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    /// <summary>Columna generada; no existe en el dominio (propiedad sombra).</summary>
    public const string EmpresaClave = "EmpresaClave";

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

        // MySQL permite varios NULL en un índice único, así que (empresa_id, nombre) no evitaba
        // dos roles globales con el mismo nombre. empresa_clave convierte el NULL en un valor fijo
        // y el índice único se hace sobre ella. La API nunca la escribe (la calcula MySQL).
        builder.Property<Guid>(EmpresaClave)
            .HasColumnName("empresa_clave")
            .HasColumnType("binary(16)")
            .HasComputedColumnSql("COALESCE(`empresa_id`, 0x00000000000000000000000000000000)", stored: true);

        // También se valida en Application para devolver un 409 con un mensaje claro.
        builder.HasIndex(EmpresaClave, nameof(Rol.Nombre))
            .IsUnique()
            .HasDatabaseName("uq_rol_empresa_clave_nombre");
    }
}
