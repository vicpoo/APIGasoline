using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class DispositivoEmpleadoConfiguration : IEntityTypeConfiguration<DispositivoEmpleado>
{
    public void Configure(EntityTypeBuilder<DispositivoEmpleado> builder)
    {
        builder.ToTable("dispositivo_empleado");

        builder.HasKey(d => d.DispositivoEmpleadoId);
        builder.Property(d => d.DispositivoEmpleadoId)
            .HasColumnName("dispositivo_empleado_id")
            .ValueGeneratedNever();

        builder.Property(d => d.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(d => d.EmpleadoId).HasColumnName("empleado_id").IsRequired();

        builder.Property(d => d.TokenFcm)
            .HasColumnName("token_fcm")
            .HasMaxLength(DispositivoEmpleado.TokenMaxLength)
            .IsRequired();

        // enum('android','ios','web') en MySQL
        builder.Property(d => d.Plataforma)
            .HasColumnName("plataforma")
            .HasColumnType("enum('android','ios','web')")
            .HasConversion(
                p => p == PlataformaDispositivo.Android ? "android"
                   : p == PlataformaDispositivo.Ios ? "ios"
                   : "web",
                s => s == "android" ? PlataformaDispositivo.Android
                   : s == "ios" ? PlataformaDispositivo.Ios
                   : PlataformaDispositivo.Web)
            .IsRequired();

        builder.Property(d => d.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(d => d.FechaRegistro)
            .HasColumnName("fecha_registro")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(d => d.FechaUltimoUso)
            .HasColumnName("fecha_ultimo_uso")
            .HasColumnType("timestamp");

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(d => d.EmpresaId)
            .HasConstraintName("fk_dispositivo_empleado_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        // Cascade: si se borra el empleado, sus celulares ya no sirven
        builder.HasOne<Empleado>()
            .WithMany()
            .HasForeignKey(d => new { d.EmpresaId, d.EmpleadoId })
            .HasPrincipalKey(e => new { e.EmpresaId, e.EmpleadoId })
            .HasConstraintName("fk_dispositivo_empleado_empleado")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.TokenFcm)
            .IsUnique()
            .HasDatabaseName("uq_dispositivo_empleado_token_fcm");

        builder.HasIndex(d => d.EmpresaId)
            .HasDatabaseName("ix_dispositivo_empleado_empresa_id");

        builder.HasIndex(d => new { d.EmpresaId, d.DispositivoEmpleadoId })
            .IsUnique()
            .HasDatabaseName("uq_dispositivo_empleado_empresa_id_dispositivo_empleado_id");

        builder.HasIndex(d => new { d.EmpresaId, d.EmpleadoId })
            .HasDatabaseName("ix_dispositivo_empleado_empresa_id_empleado_id");

        builder.HasIndex(d => new { d.EmpleadoId, d.Activo })
            .HasDatabaseName("ix_dispositivo_empleado_empleado_id_activo");
    }
}
