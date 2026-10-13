using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class SolicitudReservaConfiguration : IEntityTypeConfiguration<SolicitudReserva>
{
    public void Configure(EntityTypeBuilder<SolicitudReserva> builder)
    {
        builder.ToTable("solicitud_reserva");

        builder.HasKey(s => s.SolicitudReservaId);
        builder.Property(s => s.SolicitudReservaId)
            .HasColumnName("solicitud_reserva_id")
            .ValueGeneratedNever();

        builder.Property(s => s.EmpresaId)
            .HasColumnName("empresa_id")
            .IsRequired();

        builder.Property(s => s.CarroId)
            .HasColumnName("carro_id")
            .IsRequired();

        builder.Property(s => s.EmpleadoSolicitanteId)
            .HasColumnName("empleado_solicitante_id")
            .IsRequired();

        // La BD la llena al insertar; EF la lee de vuelta y nunca la envía.
        builder.Property(s => s.FechaSolicitud)
            .HasColumnName("fecha_solicitud")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Estado)
            .HasColumnName("estado")
            .HasColumnType("enum('pendiente','aprobada','rechazada','cancelada','finalizada')")
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<EstadoSolicitudReserva>(v, true))
            .HasDefaultValueSql("'pendiente'")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(s => s.EmpleadoConfirmadorId)
            .HasColumnName("empleado_confirmador_id");

        builder.Property(s => s.FechaResolucion)
            .HasColumnName("fecha_resolucion")
            .HasColumnType("timestamp");

        builder.Property(s => s.MotivoResolucion)
            .HasColumnName("motivo_resolucion")
            .HasMaxLength(SolicitudReserva.MotivoMaxLength);

        // Liberación del carro (HU-USO-04 / HU-EST-02)
        builder.Property(s => s.FechaLiberacion)
            .HasColumnName("fecha_liberacion")
            .HasColumnType("timestamp");

        builder.Property(s => s.ObservacionesLiberacion)
            .HasColumnName("observaciones_liberacion")
            .HasMaxLength(SolicitudReserva.ObservacionesMaxLength);

        // Propiedades calculadas en el dominio, no son columnas
        builder.Ignore(s => s.EstaPendiente);
        builder.Ignore(s => s.EstaAprobada);

        // Restrict en todas: el historial de solicitudes no debe perderse en cascada.
        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(s => s.EmpresaId)
            .HasConstraintName("fk_solicitud_reserva_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Carro>()
            .WithMany()
            .HasForeignKey(s => new { s.EmpresaId, s.CarroId })
            .HasPrincipalKey(c => new { c.EmpresaId, c.CarroId })
            .HasConstraintName("fk_solicitud_reserva_carro")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Empleado>()
            .WithMany()
            .HasForeignKey(s => new { s.EmpresaId, s.EmpleadoSolicitanteId })
            .HasPrincipalKey(e => new { e.EmpresaId, e.EmpleadoId })
            .HasConstraintName("fk_solicitud_reserva_empleado_solicitante")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Empleado>()
            .WithMany()
            .HasForeignKey(s => new { s.EmpresaId, s.EmpleadoConfirmadorId })
            .HasPrincipalKey(e => new { e.EmpresaId, e.EmpleadoId })
            .HasConstraintName("fk_solicitud_reserva_empleado_confirmador")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.EmpresaId)
            .HasDatabaseName("ix_solicitud_reserva_empresa_id");

        // Llave alterna: es la que referencian las FK compuestas (tenant-safe)
        builder.HasAlternateKey(s => new { s.EmpresaId, s.SolicitudReservaId })
            .HasName("uq_solicitud_reserva_empresa_id_solicitud_reserva_id");

        // Índices de las llaves foráneas (EF los crearía igual; aquí les damos nombre)
        builder.HasIndex(s => new { s.EmpresaId, s.CarroId })
            .HasDatabaseName("ix_solicitud_reserva_empresa_id_carro_id");

        // v4: reserva aprobada o pendientes de un carro
        builder.HasIndex(s => new { s.EmpresaId, s.CarroId, s.Estado })
            .HasDatabaseName("ix_solicitud_reserva_empresa_id_carro_id_estado");

        builder.HasIndex(s => new { s.EmpresaId, s.EmpleadoSolicitanteId })
            .HasDatabaseName("ix_solicitud_reserva_empresa_id_empleado_solicitante_id");

        builder.HasIndex(s => new { s.EmpresaId, s.EmpleadoConfirmadorId })
            .HasDatabaseName("ix_solicitud_reserva_empresa_id_empleado_confirmador_id");
    }
}
