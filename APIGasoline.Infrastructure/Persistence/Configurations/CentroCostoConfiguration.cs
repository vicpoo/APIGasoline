using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class CentroCostoConfiguration : IEntityTypeConfiguration<CentroCosto>
{
    public void Configure(EntityTypeBuilder<CentroCosto> builder)
    {
        builder.ToTable("centro_costo");

        builder.HasKey(cc => cc.CentroCostoId);
        builder.Property(cc => cc.CentroCostoId)
            .HasColumnName("centro_costo_id")
            .ValueGeneratedNever();

        builder.Property(cc => cc.EmpresaId)
            .HasColumnName("empresa_id")
            .IsRequired();

        builder.Property(cc => cc.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(CentroCosto.NombreMaxLength)
            .IsRequired();

        builder.Property(cc => cc.Clave)
            .HasColumnName("clave")
            .HasMaxLength(CentroCosto.ClaveMaxLength)
            .IsRequired();

        // Default en BD = true, pero EF siempre envía el valor
        // (si no, un "false" se ignoraría y la BD guardaría true).
        builder.Property(cc => cc.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .ValueGeneratedNever()
            .IsRequired();

        // La BD la llena al insertar; EF la lee de vuelta y nunca la envía.
        builder.Property(cc => cc.FechaRegistro)
            .HasColumnName("fecha_registro")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        // Restrict: la empresa usa soft delete, nunca se borra físicamente con centros de costo.
        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(cc => cc.EmpresaId)
            .HasConstraintName("fk_centro_costo_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cc => cc.EmpresaId)
            .HasDatabaseName("ix_centro_costo_empresa_id");

        builder.HasIndex(cc => new { cc.EmpresaId, cc.CentroCostoId })
            .IsUnique()
            .HasDatabaseName("uq_centro_costo_empresa_id_centro_costo_id");

        builder.HasIndex(cc => new { cc.EmpresaId, cc.Clave })
            .IsUnique()
            .HasDatabaseName("uq_centro_costo_empresa_id_clave");
    }
}
