using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace APIGasoline.Infrastructure.Persistence.Configurations;

public class SesionUsuarioConfiguration : IEntityTypeConfiguration<SesionUsuario>
{
    public void Configure(EntityTypeBuilder<SesionUsuario> builder)
    {
        builder.ToTable("sesion_usuario");

        builder.HasKey(s => s.SesionUsuarioId);
        builder.Property(s => s.SesionUsuarioId)
            .HasColumnName("sesion_usuario_id")
            .ValueGeneratedNever();

        builder.Property(s => s.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(s => s.EmpresaId).HasColumnName("empresa_id");

        // Concurrency token: si dos renovaciones con el mismo token llegan al mismo tiempo,
        // el UPDATE lleva "WHERE token_hash = <el original>" y solo una de las dos gana.
        builder.Property(s => s.TokenHash)
            .HasColumnName("token_hash")
            .HasColumnType("char(64)")
            .HasMaxLength(SesionUsuario.TokenHashLength)
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(s => s.TokenHashAnterior)
            .HasColumnName("token_hash_anterior")
            .HasColumnType("char(64)")
            .HasMaxLength(SesionUsuario.TokenHashLength);

        // Valores fijos: 'android','ios','web' (varchar + CHECK) (igual que dispositivo_empleado)
        builder.Property(s => s.Plataforma)
            .HasColumnName("plataforma")
            .EsEnum("android", "ios", "web")
            .HasConversion(new ValueConverter<PlataformaDispositivo, string>(
                p => p == PlataformaDispositivo.Android ? "android"
                   : p == PlataformaDispositivo.Ios ? "ios"
                   : "web",
                s => s == "android" ? PlataformaDispositivo.Android
                   : s == "ios" ? PlataformaDispositivo.Ios
                   : PlataformaDispositivo.Web));

        builder.Property(s => s.NombreDispositivo)
            .HasColumnName("nombre_dispositivo")
            .HasMaxLength(SesionUsuario.NombreDispositivoMaxLength);

        builder.Property(s => s.Ip)
            .HasColumnName("ip")
            .HasMaxLength(SesionUsuario.IpMaxLength);

        builder.Property(s => s.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(SesionUsuario.UserAgentMaxLength);

        builder.Property(s => s.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .HasColumnType(TiposSql.FechaHora)
            .IsRequired();

        builder.Property(s => s.FechaUltimoUso)
            .HasColumnName("fecha_ultimo_uso")
            .HasColumnType(TiposSql.FechaHora)
            .IsRequired();

        builder.Property(s => s.FechaRotacion)
            .HasColumnName("fecha_rotacion")
            .HasColumnType(TiposSql.FechaHora);

        builder.Property(s => s.ExpiraEn)
            .HasColumnName("expira_en")
            .HasColumnType(TiposSql.FechaHora)
            .IsRequired();

        builder.Property(s => s.ExpiraAbsolutaEn)
            .HasColumnName("expira_absoluta_en")
            .HasColumnType(TiposSql.FechaHora)
            .IsRequired();

        builder.Property(s => s.FechaRevocacion)
            .HasColumnName("fecha_revocacion")
            .HasColumnType(TiposSql.FechaHora);

        builder.Property(s => s.MotivoRevocacion)
            .HasColumnName("motivo_revocacion")
            .EsEnum(SnakeCaseEnumConverter<MotivoRevocacionSesion>.Valores)
            .HasConversion(new SnakeCaseEnumConverter<MotivoRevocacionSesion>());

        builder.Property(s => s.RevocadaPorUsuarioId).HasColumnName("revocada_por_usuario_id");

        // Propiedad calculada en el dominio, no es columna
        builder.Ignore(s => s.EstaRevocada);

        // Cascade: si se borra el usuario, sus sesiones ya no sirven
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(s => s.UsuarioId)
            .HasConstraintName("fk_sesion_usuario_usuario")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.TokenHash)
            .IsUnique()
            .HasDatabaseName("uq_sesion_usuario_token_hash");

        builder.HasIndex(s => s.TokenHashAnterior)
            .HasDatabaseName("ix_sesion_usuario_token_hash_anterior");

        builder.HasIndex(s => new { s.UsuarioId, s.FechaRevocacion, s.ExpiraEn })
            .HasDatabaseName("ix_sesion_usuario_usuario_id_fecha_revocacion_expira_en");

        builder.HasIndex(s => new { s.EmpresaId, s.FechaRevocacion })
            .HasDatabaseName("ix_sesion_usuario_empresa_id_fecha_revocacion");

        builder.HasIndex(s => s.ExpiraEn)
            .HasDatabaseName("ix_sesion_usuario_expira_en");
    }
}
