using System;
using System.Collections.Generic;
using System.Text;

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
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("uq_usuario_email");

        builder.Property(u => u.Estado)
            .HasColumnName("estado")
            .HasColumnType("enum('activo','inactivo')")
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<EstadoUsuario>(v, true))
            .HasDefaultValueSql("'activo'")
            .IsRequired();
    }
}