using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class OutboxMensajeConfiguration : IEntityTypeConfiguration<OutboxMensaje>
{
    public void Configure(EntityTypeBuilder<OutboxMensaje> builder)
    {
        builder.ToTable("outbox_mensaje");

        builder.HasKey(o => o.OutboxMensajeId);
        builder.Property(o => o.OutboxMensajeId)
            .HasColumnName("outbox_mensaje_id")
            .ValueGeneratedNever();

        builder.Property(o => o.EmpresaId).HasColumnName("empresa_id").IsRequired();

        builder.Property(o => o.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(OutboxMensaje.TipoMaxLength)
            .IsRequired();

        builder.Property(o => o.AgregadoId).HasColumnName("agregado_id").IsRequired();

        builder.Property(o => o.Payload)
            .HasColumnName("payload")
            .EsJson()
            .IsRequired();

        // Valores fijos: 'pendiente','procesando','enviado','fallido' (varchar + CHECK)
        builder.Property(o => o.Estado)
            .HasColumnName("estado")
            .EsEnum("pendiente", "procesando", "enviado", "fallido")
            .HasConversion(
                e => e == EstadoOutbox.Pendiente ? "pendiente"
                   : e == EstadoOutbox.Procesando ? "procesando"
                   : e == EstadoOutbox.Enviado ? "enviado"
                   : "fallido",
                s => s == "pendiente" ? EstadoOutbox.Pendiente
                   : s == "procesando" ? EstadoOutbox.Procesando
                   : s == "enviado" ? EstadoOutbox.Enviado
                   : EstadoOutbox.Fallido)
            .HasDefaultValueSql("'pendiente'")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(o => o.Intentos)
            .HasColumnName("intentos")
            .HasDefaultValue(0)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(o => o.ProximoIntento)
            .HasColumnName("proximo_intento")
            .HasColumnType(TiposSql.FechaHora)
            .HasDefaultValueSql(TiposSql.Ahora)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(o => o.UltimoError)
            .HasColumnName("ultimo_error")
            .HasMaxLength(OutboxMensaje.UltimoErrorMaxLength);

        builder.Property(o => o.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .HasColumnType(TiposSql.FechaHora)
            .HasDefaultValueSql(TiposSql.Ahora)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(o => o.FechaProcesado)
            .HasColumnName("fecha_procesado")
            .HasColumnType(TiposSql.FechaHora);

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(o => o.EmpresaId)
            .HasConstraintName("fk_outbox_mensaje_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => o.EmpresaId)
            .HasDatabaseName("ix_outbox_mensaje_empresa_id");

        builder.HasIndex(o => new { o.EmpresaId, o.OutboxMensajeId })
            .IsUnique()
            .HasDatabaseName("uq_outbox_mensaje_empresa_id_outbox_mensaje_id");

        // Consulta del worker: pendientes cuya hora de intento ya pasó
        builder.HasIndex(o => new { o.Estado, o.ProximoIntento })
            .HasDatabaseName("ix_outbox_mensaje_estado_proximo_intento");

        builder.HasIndex(o => o.AgregadoId)
            .HasDatabaseName("ix_outbox_mensaje_agregado_id");
    }
}
