using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresa");

        builder.HasKey(e => e.EmpresaId);
        builder.Property(e => e.EmpresaId)
            .HasColumnName("empresa_id")
            .ValueGeneratedNever();

        builder.Property(e => e.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(Empresa.NombreMaxLength)
            .IsRequired();

        // Soft delete. v4: la columna se renombra de "delete" (palabra reservada) a "eliminado".
        builder.Property(e => e.Eliminado)
            .HasColumnName("eliminado")
            .HasDefaultValue(false)
            .ValueGeneratedNever()
            .IsRequired();

        // Empresa activa o inactiva (HU-ADM-01)
        builder.Property(e => e.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .ValueGeneratedNever()
            .IsRequired();

        // La BD la llena al insertar; EF la lee de vuelta y nunca la envía.
        builder.Property(e => e.FechaRegistro)
            .HasColumnName("fecha_registro")
            .HasColumnType(TiposSql.FechaHora)
            .HasDefaultValueSql(TiposSql.Ahora)
            .ValueGeneratedOnAdd();

        // Filtro global con nombre: todas las consultas ignoran las empresas eliminadas.
        // (El filtro por empresa se agrega en AppDbContext.)
        builder.HasQueryFilter(FiltrosConsulta.Eliminado, e => !e.Eliminado);
    }
}
