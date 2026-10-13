using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class RevisionAuditoriaConfiguration : IEntityTypeConfiguration<RevisionAuditoria>
{
    public void Configure(EntityTypeBuilder<RevisionAuditoria> builder)
    {
        builder.ToTable("revision_auditoria");

        builder.HasKey(r => r.RevisionAuditoriaId);

        builder.Property(r => r.RevisionAuditoriaId)
            .HasColumnName("revision_auditoria_id")
            .ValueGeneratedNever();

        builder.Property(r => r.EmpresaId).HasColumnName("empresa_id").IsRequired();

        // Alcance: un viaje, o un carro en un periodo
        builder.Property(r => r.ViajeId).HasColumnName("viaje_id");
        builder.Property(r => r.CarroId).HasColumnName("carro_id");
        builder.Property(r => r.PeriodoDesde).HasColumnName("periodo_desde").HasColumnType("date");
        builder.Property(r => r.PeriodoHasta).HasColumnName("periodo_hasta").HasColumnType("date");

        builder.Property(r => r.EmpleadoRevisorId).HasColumnName("empleado_revisor_id").IsRequired();

        builder.Property(r => r.FechaRevision)
            .HasColumnName("fecha_revision")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.Property(r => r.EstadoRevision)
            .HasColumnName("estado_revision")
            .HasColumnType(SnakeCaseEnumConverter<EstadoRevision>.ColumnType)
            .HasConversion(new SnakeCaseEnumConverter<EstadoRevision>())
            .HasDefaultValueSql("'pendiente_informacion'")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(r => r.Observaciones)
            .HasColumnName("observaciones")
            .HasMaxLength(RevisionAuditoria.ObservacionesMaxLength);

        builder.Property(r => r.FechaCierre)
            .HasColumnName("fecha_cierre")
            .HasColumnType("timestamp");

        // Propiedad calculada en el dominio, no es columna
        builder.Ignore(r => r.EstaCerrada);

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(r => r.EmpresaId)
            .HasConstraintName("fk_revision_auditoria_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Viaje>()
            .WithMany()
            .HasForeignKey(r => new { r.EmpresaId, r.ViajeId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.ViajeId })
            .HasConstraintName("fk_revision_auditoria_viaje")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Carro>()
            .WithMany()
            .HasForeignKey(r => new { r.EmpresaId, r.CarroId })
            .HasPrincipalKey(c => new { c.EmpresaId, c.CarroId })
            .HasConstraintName("fk_revision_auditoria_carro")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Empleado>()
            .WithMany()
            .HasForeignKey(r => new { r.EmpresaId, r.EmpleadoRevisorId })
            .HasPrincipalKey(e => new { e.EmpresaId, e.EmpleadoId })
            .HasConstraintName("fk_revision_auditoria_empleado_revisor")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.EmpresaId)
            .HasDatabaseName("ix_revision_auditoria_empresa_id");

        builder.HasIndex(r => new { r.EmpresaId, r.RevisionAuditoriaId })
            .IsUnique()
            .HasDatabaseName("uq_revision_auditoria_empresa_id_revision_auditoria_id");

        builder.HasIndex(r => new { r.EmpresaId, r.ViajeId })
            .HasDatabaseName("ix_revision_auditoria_empresa_id_viaje_id");

        builder.HasIndex(r => new { r.EmpresaId, r.CarroId, r.PeriodoDesde })
            .HasDatabaseName("ix_revision_auditoria_empresa_id_carro_id_periodo_desde");

        builder.HasIndex(r => new { r.EmpresaId, r.EstadoRevision })
            .HasDatabaseName("ix_revision_auditoria_empresa_id_estado_revision");

        builder.HasIndex(r => new { r.EmpresaId, r.EmpleadoRevisorId })
            .HasDatabaseName("ix_revision_auditoria_empresa_id_empleado_revisor_id");

        // Regla del alcance: un viaje, o un carro con su periodo completo
        builder.ToTable(t => t.HasCheckConstraint(
            "ck_revision_auditoria_alcance",
            "(viaje_id IS NOT NULL) OR (carro_id IS NOT NULL AND periodo_desde IS NOT NULL AND periodo_hasta IS NOT NULL)"));
    }
}
