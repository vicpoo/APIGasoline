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

        // Opcionales: un centro puede no pertenecer a ningún corporativo o división
        builder.Property(cc => cc.CorporativoId)
            .HasColumnName("corporativo_id");

        builder.Property(cc => cc.DivisionId)
            .HasColumnName("division_id");

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
            .HasColumnType(TiposSql.FechaHora)
            .HasDefaultValueSql(TiposSql.Ahora)
            .ValueGeneratedOnAdd();

        // Restrict: la empresa usa soft delete, nunca se borra físicamente con centros de costo.
        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(cc => cc.EmpresaId)
            .HasConstraintName("fk_centro_costo_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cc => cc.EmpresaId)
            .HasDatabaseName("ix_centro_costo_empresa_id");

        // Llave alterna: la referencian viaje y carga_combustible con FK compuesta (tenant-safe)
        builder.HasAlternateKey(cc => new { cc.EmpresaId, cc.CentroCostoId })
            .HasName("uq_centro_costo_empresa_id_centro_costo_id");

        // FK compuestas con empresa_id: el corporativo y la división deben ser de la misma empresa.
        // SQL Server no revisa una FK compuesta si alguna columna es NULL, así que son opcionales.
        builder.HasOne<Corporativo>()
            .WithMany()
            .HasForeignKey(cc => new { cc.EmpresaId, cc.CorporativoId })
            .HasPrincipalKey(c => new { c.EmpresaId, c.CorporativoId })
            .HasConstraintName("fk_centro_costo_corporativo")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Division>()
            .WithMany()
            .HasForeignKey(cc => new { cc.EmpresaId, cc.DivisionId })
            .HasPrincipalKey(d => new { d.EmpresaId, d.DivisionId })
            .HasConstraintName("fk_centro_costo_division")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Con los dos llenos, la pareja corporativo + división debe estar ligada
        builder.HasOne<CorporativoDivision>()
            .WithMany()
            .HasForeignKey(cc => new { cc.EmpresaId, cc.CorporativoId, cc.DivisionId })
            .HasPrincipalKey(l => new { l.EmpresaId, l.CorporativoId, l.DivisionId })
            .HasConstraintName("fk_centro_costo_corporativo_division")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cc => new { cc.EmpresaId, cc.CorporativoId, cc.DivisionId })
            .HasDatabaseName("ix_centro_costo_empresa_id_corporativo_id_division_id");

        builder.HasIndex(cc => new { cc.EmpresaId, cc.DivisionId })
            .HasDatabaseName("ix_centro_costo_empresa_id_division_id");

        builder.HasIndex(cc => new { cc.EmpresaId, cc.Clave })
            .IsUnique()
            .HasDatabaseName("uq_centro_costo_empresa_id_clave");
    }
}
