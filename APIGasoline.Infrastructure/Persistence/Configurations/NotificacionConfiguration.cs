using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("notificacion");

        builder.HasKey(n => n.NotificacionId);
        builder.Property(n => n.NotificacionId)
            .HasColumnName("notificacion_id")
            .ValueGeneratedNever();

        builder.Property(n => n.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(n => n.EmpleadoDestinatarioId).HasColumnName("empleado_destinatario_id").IsRequired();

        // enum('nueva_solicitud','resultado_solicitud','cambio_estatus_forzado','reporte_mantenimiento') en MySQL
        builder.Property(n => n.Tipo)
            .HasColumnName("tipo")
            .HasColumnType("enum('nueva_solicitud','resultado_solicitud','cambio_estatus_forzado','reporte_mantenimiento')")
            .HasConversion(
                v => v == TipoNotificacion.NuevaSolicitud ? "nueva_solicitud"
                   : v == TipoNotificacion.ResultadoSolicitud ? "resultado_solicitud"
                   : v == TipoNotificacion.CambioEstatusForzado ? "cambio_estatus_forzado"
                   : "reporte_mantenimiento",
                v => v == "nueva_solicitud" ? TipoNotificacion.NuevaSolicitud
                   : v == "resultado_solicitud" ? TipoNotificacion.ResultadoSolicitud
                   : v == "cambio_estatus_forzado" ? TipoNotificacion.CambioEstatusForzado
                   : TipoNotificacion.ReporteMantenimiento)
            .IsRequired();

        builder.Property(n => n.ReferenciaId).HasColumnName("referencia_id");

        builder.Property(n => n.Mensaje)
            .HasColumnName("mensaje")
            .HasMaxLength(Notificacion.MensajeMaxLength);

        builder.Property(n => n.Leida)
            .HasColumnName("leida")
            .HasDefaultValue(false)
            .ValueGeneratedNever()
            .IsRequired();

        // La BD la llena al insertar; EF la lee de vuelta y nunca la envía.
        builder.Property(n => n.FechaEnvio)
            .HasColumnName("fecha_envio")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(n => n.EmpresaId)
            .HasConstraintName("fk_notificacion_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Empleado>()
            .WithMany()
            .HasForeignKey(n => new { n.EmpresaId, n.EmpleadoDestinatarioId })
            .HasPrincipalKey(e => new { e.EmpresaId, e.EmpleadoId })
            .HasConstraintName("fk_notificacion_empleado_destinatario")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => n.EmpresaId)
            .HasDatabaseName("ix_notificacion_empresa_id");

        builder.HasIndex(n => new { n.EmpresaId, n.NotificacionId })
            .IsUnique()
            .HasDatabaseName("uq_notificacion_empresa_id_notificacion_id");

        builder.HasIndex(n => new { n.EmpresaId, n.EmpleadoDestinatarioId })
            .HasDatabaseName("ix_notificacion_empresa_id_empleado_destinatario_id");

        // Para la llave foránea y para la bandeja/contador de no leídas de un empleado
        builder.HasIndex(n => new { n.EmpleadoDestinatarioId, n.Leida })
            .HasDatabaseName("ix_notificacion_empleado_destinatario_id_leida");
    }
}
