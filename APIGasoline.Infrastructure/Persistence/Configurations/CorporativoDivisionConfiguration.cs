using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

/// <summary>
/// Liga N:M entre corporativos y divisiones. Las dos FK son compuestas con empresa_id,
/// así la BD impide ligar un corporativo con una división de otra empresa.
/// </summary>
public class CorporativoDivisionConfiguration : IEntityTypeConfiguration<CorporativoDivision>
{
    public void Configure(EntityTypeBuilder<CorporativoDivision> builder)
    {
        builder.ToTable("corporativo_division");

        builder.HasKey(l => l.CorporativoDivisionId);
        builder.Property(l => l.CorporativoDivisionId)
            .HasColumnName("corporativo_division_id")
            .ValueGeneratedNever();

        builder.Property(l => l.EmpresaId)
            .HasColumnName("empresa_id")
            .IsRequired();

        builder.Property(l => l.CorporativoId)
            .HasColumnName("corporativo_id")
            .IsRequired();

        builder.Property(l => l.DivisionId)
            .HasColumnName("division_id")
            .IsRequired();

        // La BD la llena al insertar; EF la lee de vuelta y nunca la envía.
        builder.Property(l => l.FechaRegistro)
            .HasColumnName("fecha_registro")
            .HasColumnType(TiposSql.FechaHora)
            .HasDefaultValueSql(TiposSql.Ahora)
            .ValueGeneratedOnAdd();

        // Restrict: no se borra un corporativo o una división que todavía tiene ligas.
        builder.HasOne<Corporativo>()
            .WithMany()
            .HasForeignKey(l => new { l.EmpresaId, l.CorporativoId })
            .HasPrincipalKey(c => new { c.EmpresaId, c.CorporativoId })
            .HasConstraintName("fk_corporativo_division_corporativo")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Division>()
            .WithMany()
            .HasForeignKey(l => new { l.EmpresaId, l.DivisionId })
            .HasPrincipalKey(d => new { d.EmpresaId, d.DivisionId })
            .HasConstraintName("fk_corporativo_division_division")
            .OnDelete(DeleteBehavior.Restrict);

        // Un corporativo se liga una sola vez con cada división
        builder.HasIndex(l => new { l.CorporativoId, l.DivisionId })
            .IsUnique()
            .HasDatabaseName("uq_corporativo_division_corporativo_id_division_id");

        builder.HasIndex(l => new { l.EmpresaId, l.DivisionId })
            .HasDatabaseName("ix_corporativo_division_empresa_id_division_id");

        // Llave alterna: la referencia centro_costo para exigir que su pareja esté ligada
        builder.HasAlternateKey(l => new { l.EmpresaId, l.CorporativoId, l.DivisionId })
            .HasName("uq_corporativo_division_empresa_id_corporativo_id_division_id");
    }
}
