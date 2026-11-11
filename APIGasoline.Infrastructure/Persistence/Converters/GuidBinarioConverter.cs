using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace APIGasoline.Infrastructure.Persistence.Converters;

/// <summary>
/// Guid ⇄ binary(16) en orden RFC 4122 (big-endian), el mismo formato que usaba MySQL.
///
/// Por qué no uniqueidentifier: los Id son UUID v7 (ordenados por tiempo) y la API ordena por Id
/// para desempatar (puntos GPS del mismo segundo, listas, bitácoras). SQL Server ordena los
/// uniqueidentifier empezando por los últimos 6 bytes, así que ese orden se perdería y el índice
/// de la PK se fragmentaría. En binary(16) big-endian el orden de bytes es el orden de creación.
/// </summary>
public sealed class GuidBinarioConverter : ValueConverter<Guid, byte[]>
{
    public const string ColumnType = "binary(16)";

    public GuidBinarioConverter()
        : base(v => ABytes(v), v => AGuid(v))
    {
    }

    public static byte[] ABytes(Guid valor) => valor.ToByteArray(bigEndian: true);

    public static Guid AGuid(byte[] valor) => new(valor, bigEndian: true);
}
