using APIGasoline.Domain.Entities;
using APIGasoline.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class CambioEstatusCarroConfiguration : IEntityTypeConfiguration<CambioEstatusCarro>
{
    public void Configure(EntityTypeBuilder<CambioEstatusCarro> builder)
    {
        builder.ToTable("cambio_estatus_carro");

        builder.HasKey(c => c.CambioEstatusCarroId);
        builder.Property(c => c.CambioEstatusCarroId)
            .HasColumnName("cambio_estatus_carro_id")
            .ValueGeneratedNever();

        builder.Property(c => c.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(c => c.CarroId).HasColumnName("carro_id").IsRequired();
        builder.Property(c => c.EmpleadoId).HasColumnName("empleado_id").IsRequired();
        builder.Property(c => c.ViajeAfectadoId).HasColumnName("viaje_afectado_id");

        builder.Property(c => c.EstatusAnterior)
            .HasColumnName("estatus_anterior")
            .HasColumnType(EstadoCarroConverter.ColumnType)
            .HasConversion(new EstadoCarroConverter())
            .IsRequired();

        builder.Property(c => c.EstatusNuevo)
            .HasColumnName("estatus_nuevo")
            .HasColumnType(EstadoCarroConverter.ColumnType)
            .HasConversion(new EstadoCarroConverter())
            .IsRequired();

        builder.Property(c => c.Motivo)
            .HasColumnName("motivo")
            .HasMaxLength(CambioEstatusCarro.MotivoMaxLength);

        // Si la app manda la fecha (offline) se usa; si viene null, la pone la BD.
        builder.Property(c => c.FechaCambio)
            .HasColumnName("fecha_cambio")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Sincronizado)
            .HasColumnName("sincronizado")
            .HasDefaultValue(false)
            .ValueGeneratedNever()
            .IsRequired();

        // Restrict en todas: es una bitácora, no debe perderse en cascada.
        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .HasConstraintName("fk_cambio_estatus_carro_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Carro>()
            .WithMany()
            .HasForeignKey(c => new { c.EmpresaId, c.CarroId })
            .HasPrincipalKey(ca => new { ca.EmpresaId, ca.CarroId })
            .HasConstraintName("fk_cambio_estatus_carro_carro")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Empleado>()
            .WithMany()
            .HasForeignKey(c => new { c.EmpresaId, c.EmpleadoId })
            .HasPrincipalKey(e => new { e.EmpresaId, e.EmpleadoId })
            .HasConstraintName("fk_cambio_estatus_carro_empleado")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Viaje>()
            .WithMany()
            .HasForeignKey(c => new { c.EmpresaId, c.ViajeAfectadoId })
            .HasPrincipalKey(v => new { v.EmpresaId, v.ViajeId })
            .HasConstraintName("fk_cambio_estatus_carro_viaje")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.EmpresaId)
            .HasDatabaseName("ix_cambio_estatus_carro_empresa_id");

        builder.HasIndex(c => new { c.EmpresaId, c.CambioEstatusCarroId })
            .IsUnique()
            .HasDatabaseName("uq_cambio_estatus_carro_empresa_id_cambio_estatus_carro_id");

        builder.HasIndex(c => new { c.EmpresaId, c.CarroId })
            .HasDatabaseName("ix_cambio_estatus_carro_empresa_id_carro_id");

        // Índices de las llaves foráneas (EF los crearía igual; aquí les damos nombre)
        builder.HasIndex(c => new { c.EmpresaId, c.EmpleadoId }).HasDatabaseName("ix_cambio_estatus_carro_empresa_id_empleado_id");
        builder.HasIndex(c => new { c.EmpresaId, c.ViajeAfectadoId }).HasDatabaseName("ix_cambio_estatus_carro_empresa_id_viaje_afectado_id");
    }
}
