using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class ViajeConfiguration : IEntityTypeConfiguration<Viaje>
{
    public void Configure(EntityTypeBuilder<Viaje> builder)
    {
        builder.ToTable("viaje");

        builder.HasKey(v => v.ViajeId);
        builder.Property(v => v.ViajeId)
            .HasColumnName("viaje_id")
            .ValueGeneratedOnAdd();

        builder.Property(v => v.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(v => v.CarroId).HasColumnName("carro_id").IsRequired();
        builder.Property(v => v.SolicitudReservaId).HasColumnName("solicitud_reserva_id");
        builder.Property(v => v.EmpleadoOperadorId).HasColumnName("empleado_operador_id").IsRequired();

        // Inicio (la fecha la manda la app: puede registrarse offline)
        builder.Property(v => v.FechaInicio)
            .HasColumnName("fecha_inicio")
            .HasColumnType("timestamp")
            .IsRequired();

        builder.Property(v => v.UbicacionInicioLat).HasColumnName("ubicacion_inicio_lat").HasPrecision(9, 6);
        builder.Property(v => v.UbicacionInicioLng).HasColumnName("ubicacion_inicio_lng").HasPrecision(9, 6);

        builder.Property(v => v.KilometrajeInicial)
            .HasColumnName("kilometraje_inicial")
            .HasPrecision(10, 2)
            .IsRequired();

        // Fin
        builder.Property(v => v.FechaFin)
            .HasColumnName("fecha_fin")
            .HasColumnType("timestamp");

        builder.Property(v => v.UbicacionFinLat).HasColumnName("ubicacion_fin_lat").HasPrecision(9, 6);
        builder.Property(v => v.UbicacionFinLng).HasColumnName("ubicacion_fin_lng").HasPrecision(9, 6);
        builder.Property(v => v.KilometrajeFinal).HasColumnName("kilometraje_final").HasPrecision(10, 2);
        builder.Property(v => v.DistanciaRecorrida).HasColumnName("distancia_recorrida").HasPrecision(10, 2);

        // enum('en_curso','finalizado','interrumpido') en MySQL
        builder.Property(v => v.Estado)
            .HasColumnName("estado")
            .HasColumnType("enum('en_curso','finalizado','interrumpido')")
            .HasConversion(
                e => e == EstadoViaje.EnCurso ? "en_curso"
                   : e == EstadoViaje.Finalizado ? "finalizado"
                   : "interrumpido",
                s => s == "en_curso" ? EstadoViaje.EnCurso
                   : s == "finalizado" ? EstadoViaje.Finalizado
                   : EstadoViaje.Interrumpido)
            .HasDefaultValueSql("'en_curso'")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(v => v.MotivoInterrupcion)
            .HasColumnName("motivo_interrupcion")
            .HasMaxLength(Viaje.MotivoMaxLength);

        builder.Property(v => v.Sincronizado)
            .HasColumnName("sincronizado")
            .HasDefaultValue(false)
            .ValueGeneratedNever()
            .IsRequired();

        // Propiedad calculada en el dominio, no es columna
        builder.Ignore(v => v.EstaEnCurso);

        // Restrict en todas: el historial de viajes no debe perderse en cascada.
        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(v => v.EmpresaId)
            .HasConstraintName("fk_viaje_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Carro>()
            .WithMany()
            .HasForeignKey(v => new { v.EmpresaId, v.CarroId })
            .HasPrincipalKey(c => new { c.EmpresaId, c.CarroId })
            .HasConstraintName("fk_viaje_carro")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SolicitudReserva>()
            .WithMany()
            .HasForeignKey(v => new { v.EmpresaId, v.SolicitudReservaId })
            .HasPrincipalKey(s => new { s.EmpresaId, s.SolicitudReservaId })
            .HasConstraintName("fk_viaje_solicitud_reserva")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Empleado>()
            .WithMany()
            .HasForeignKey(v => new { v.EmpresaId, v.EmpleadoOperadorId })
            .HasPrincipalKey(e => new { e.EmpresaId, e.EmpleadoId })
            .HasConstraintName("fk_viaje_empleado_operador")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.EmpresaId)
            .HasDatabaseName("ix_viaje_empresa_id");

        // Llave alterna: es la que referencian las FK compuestas (tenant-safe)
        builder.HasAlternateKey(v => new { v.EmpresaId, v.ViajeId })
            .HasName("uq_viaje_empresa_id_viaje_id");

        // Índices de las llaves foráneas (EF los crearía igual; aquí les damos nombre)
        builder.HasIndex(v => new { v.EmpresaId, v.CarroId }).HasDatabaseName("ix_viaje_empresa_id_carro_id");
        builder.HasIndex(v => new { v.EmpresaId, v.SolicitudReservaId }).HasDatabaseName("ix_viaje_empresa_id_solicitud_reserva_id");
        builder.HasIndex(v => new { v.EmpresaId, v.EmpleadoOperadorId }).HasDatabaseName("ix_viaje_empresa_id_empleado_operador_id");
    }
}
