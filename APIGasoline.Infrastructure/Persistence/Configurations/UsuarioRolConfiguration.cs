using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> builder)
    {
        builder.ToTable("usuario_rol");

        builder.HasKey(ur => ur.UsuarioRolId);
        builder.Property(ur => ur.UsuarioRolId)
            .HasColumnName("usuario_rol_id")
            .ValueGeneratedNever();

        builder.Property(ur => ur.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(ur => ur.RolId)
            .HasColumnName("rol_id")
            .IsRequired();

        // Llaves foráneas sin propiedades de navegación (el dominio solo guarda los ids).
        // Cascade: al borrar un usuario o un rol se eliminan sus asignaciones.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(ur => ur.UsuarioId)
            .HasConstraintName("fk_usuario_rol_usuario")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Rol>()
            .WithMany()
            .HasForeignKey(ur => ur.RolId)
            .HasConstraintName("fk_usuario_rol_rol")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ur => ur.UsuarioId)
            .HasDatabaseName("ix_usuario_rol_usuario_id");

        builder.HasIndex(ur => ur.RolId)
            .HasDatabaseName("ix_usuario_rol_rol_id");

        builder.HasIndex(ur => new { ur.UsuarioId, ur.RolId })
            .IsUnique()
            .HasDatabaseName("uq_usuario_rol_usuario_id_rol_id");
    }
}
