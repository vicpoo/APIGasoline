using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class AuditoriaEventoConfiguration : IEntityTypeConfiguration<AuditoriaEvento>
{
    public void Configure(EntityTypeBuilder<AuditoriaEvento> builder)
    {
        builder.ToTable("auditoria_evento");

        builder.HasKey(a => a.AuditoriaEventoId);

        builder.Property(a => a.AuditoriaEventoId)
            .HasColumnName("auditoria_evento_id")
            .ValueGeneratedNever();

        builder.Property(a => a.EmpresaId).HasColumnName("empresa_id");
        builder.Property(a => a.UsuarioId).HasColumnName("usuario_id");

        builder.Property(a => a.Accion)
            .HasColumnName("accion")
            .HasColumnType(SnakeCaseEnumConverter<AccionAuditoria>.ColumnType)
            .HasConversion(new SnakeCaseEnumConverter<AccionAuditoria>())
            .IsRequired();

        builder.Property(a => a.Entidad)
            .HasColumnName("entidad")
            .HasMaxLength(AuditoriaEvento.EntidadMaxLength)
            .IsRequired();

        builder.Property(a => a.EntidadId)
            .HasColumnName("entidad_id")
            .IsRequired();

        builder.Property(a => a.ValoresAnteriores)
            .HasColumnName("valores_anteriores")
            .HasColumnType("json");

        builder.Property(a => a.ValoresNuevos)
            .HasColumnName("valores_nuevos")
            .HasColumnType("json");

        builder.Property(a => a.Motivo)
            .HasColumnName("motivo")
            .HasMaxLength(AuditoriaEvento.MotivoMaxLength);

        builder.Property(a => a.Origen)
            .HasColumnName("origen")
            .HasColumnType(SnakeCaseEnumConverter<OrigenAuditoria>.ColumnType)
            .HasConversion(new SnakeCaseEnumConverter<OrigenAuditoria>())
            .IsRequired();

        // Hora del servidor
        builder.Property(a => a.FechaHora)
            .HasColumnName("fecha_hora")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        // Restrict: el historial nunca se borra en cascada
        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(a => a.EmpresaId)
            .HasConstraintName("fk_auditoria_evento_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(a => a.UsuarioId)
            .HasConstraintName("fk_auditoria_evento_usuario")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.EmpresaId, a.Entidad, a.EntidadId })
            .HasDatabaseName("ix_auditoria_evento_empresa_id_entidad_entidad_id");

        builder.HasIndex(a => new { a.EmpresaId, a.FechaHora })
            .HasDatabaseName("ix_auditoria_evento_empresa_id_fecha_hora");

        builder.HasIndex(a => new { a.UsuarioId, a.FechaHora })
            .HasDatabaseName("ix_auditoria_evento_usuario_id_fecha_hora");
    }
}
