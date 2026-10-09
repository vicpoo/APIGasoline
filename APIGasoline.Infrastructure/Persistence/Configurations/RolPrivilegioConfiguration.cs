using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class RolPrivilegioConfiguration : IEntityTypeConfiguration<RolPrivilegio>
{
    public void Configure(EntityTypeBuilder<RolPrivilegio> builder)
    {
        builder.ToTable("rol_privilegio");

        builder.HasKey(rp => rp.RolPrivilegioId);
        builder.Property(rp => rp.RolPrivilegioId)
            .HasColumnName("rol_privilegio_id")
            .ValueGeneratedNever();

        builder.Property(rp => rp.RolId)
            .HasColumnName("rol_id")
            .IsRequired();

        builder.Property(rp => rp.PrivilegioId)
            .HasColumnName("privilegio_id")
            .IsRequired();

        // Llaves foráneas sin propiedades de navegación (el dominio solo guarda los ids).
        // Cascade: al borrar un rol o un privilegio se eliminan sus asignaciones.
        builder.HasOne<Rol>()
            .WithMany()
            .HasForeignKey(rp => rp.RolId)
            .HasConstraintName("fk_rol_privilegio_rol")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Privilegio>()
            .WithMany()
            .HasForeignKey(rp => rp.PrivilegioId)
            .HasConstraintName("fk_rol_privilegio_privilegio")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rp => rp.RolId)
            .HasDatabaseName("ix_rol_privilegio_rol_id");

        builder.HasIndex(rp => rp.PrivilegioId)
            .HasDatabaseName("ix_rol_privilegio_privilegio_id");

        builder.HasIndex(rp => new { rp.RolId, rp.PrivilegioId })
            .IsUnique()
            .HasDatabaseName("uq_rol_privilegio_rol_id_privilegio_id");
    }
}
