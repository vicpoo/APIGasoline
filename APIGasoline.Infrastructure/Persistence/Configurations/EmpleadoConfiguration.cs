using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("empleado");

        builder.HasKey(e => e.EmpleadoId);
        builder.Property(e => e.EmpleadoId)
            .HasColumnName("empleado_id")
            .ValueGeneratedNever();

        builder.Property(e => e.EmpresaId)
            .HasColumnName("empresa_id")
            .IsRequired();

        builder.Property(e => e.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(e => e.NombreCompleto)
            .HasColumnName("nombre_completo")
            .HasMaxLength(Empleado.NombreCompletoMaxLength)
            .IsRequired();

        builder.Property(e => e.FotoPerfil)
            .HasColumnName("foto_perfil")
            .HasMaxLength(Empleado.FotoMaxLength);

        builder.Property(e => e.FotoLicencia)
            .HasColumnName("foto_licencia")
            .HasMaxLength(Empleado.FotoMaxLength);

        // HU-LOG-01: null = licencia pendiente
        builder.Property(e => e.FechaCargaLicencia)
            .HasColumnName("fecha_carga_licencia")
            .HasColumnType("timestamp");

        // Propiedad calculada en el dominio, no es columna
        builder.Ignore(e => e.TieneLicencia);

        // Soft delete. v4: la columna se renombra de "delete" (palabra reservada) a "eliminado".
        builder.Property(e => e.Eliminado)
            .HasColumnName("eliminado")
            .HasDefaultValue(false)
            .ValueGeneratedNever()
            .IsRequired();

        // La BD la llena al insertar; EF la lee de vuelta y nunca la envía.
        builder.Property(e => e.FechaAlta)
            .HasColumnName("fecha_alta")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        // Filtro global: todas las consultas ignoran los empleados eliminados.
        builder.HasQueryFilter(e => !e.Eliminado);

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(e => e.EmpresaId)
            .HasConstraintName("fk_empleado_empresa")
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict: no se puede borrar un usuario que tiene empleado.
        // 1:1 — un usuario tiene un solo empleado (una sola empresa)
        builder.HasOne<Usuario>()
            .WithOne()
            .HasForeignKey<Empleado>(e => e.UsuarioId)
            .HasConstraintName("fk_empleado_usuario")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.UsuarioId)
            .IsUnique()
            .HasDatabaseName("uq_empleado_usuario_id");

        builder.HasIndex(e => e.EmpresaId)
            .HasDatabaseName("ix_empleado_empresa_id");

        // Llave alterna: es la que referencian las FK compuestas (tenant-safe)
        builder.HasAlternateKey(e => new { e.EmpresaId, e.EmpleadoId })
            .HasName("uq_empleado_empresa_id_empleado_id");
    }
}
