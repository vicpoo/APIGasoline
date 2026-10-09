using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario");

        builder.HasKey(u => u.UsuarioId);
        builder.Property(u => u.UsuarioId)
            .HasColumnName("usuario_id")
            .ValueGeneratedNever();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(Usuario.EmailMaxLength)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("uq_usuario_email");

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(Usuario.PasswordHashMaxLength)
            .IsRequired();

        builder.Property(u => u.Estado)
            .HasColumnName("estado")
            .HasColumnType("enum('activo','inactivo')")
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<EstadoUsuario>(v, true))
            .HasDefaultValueSql("'activo'")
            .ValueGeneratedNever()
            .IsRequired();

        // La BD la llena al insertar; EF la lee de vuelta y nunca la envía.
        builder.Property(u => u.FechaRegistro)
            .HasColumnName("fecha_registro")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        // Propiedad calculada en el dominio, no es columna
        builder.Ignore(u => u.EstaActivo);
    }
}
