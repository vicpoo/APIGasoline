using System.Reflection;
using PdfSharp.Fonts;

namespace APIGasoline.Infrastructure.Reportes;

/// <summary>
/// PDFsharp 6 no busca fuentes del sistema en Linux (el contenedor Docker no trae ninguna).
/// Este resolvedor le da la tipografía de la app (Poppins), incluida en el ensamblado como
/// recurso: cualquier familia que pida MigraDoc se dibuja con Poppins.
/// </summary>
internal sealed class FuentesReporte : IFontResolver
{
    public const string Familia = "Poppins";

    private const string Regular = "Poppins-Regular";
    private const string Negrita = "Poppins-Bold";
    private const string Seminegrita = "Poppins-SemiBold";

    private static readonly Lazy<Dictionary<string, byte[]>> Archivos = new(Cargar);

    private static readonly object Candado = new();

    /// <summary>Se llama una vez antes de generar el primer PDF (PDFsharp no deja cambiarlo después).</summary>
    public static void Registrar()
    {
        lock (Candado)
        {
            GlobalFontSettings.FontResolver ??= new FuentesReporte();
        }
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
        => new(bold ? Negrita : Regular);

    public byte[]? GetFont(string faceName)
        => Archivos.Value.TryGetValue(faceName, out var bytes) ? bytes : Archivos.Value[Regular];

    private static Dictionary<string, byte[]> Cargar()
    {
        var ensamblado = typeof(FuentesReporte).Assembly;
        return new Dictionary<string, byte[]>
        {
            [Regular] = Leer(ensamblado, "poppins_regular.ttf"),
            [Negrita] = Leer(ensamblado, "poppins_bold.ttf"),
            [Seminegrita] = Leer(ensamblado, "poppins_semibold.ttf"),
        };
    }

    private static byte[] Leer(Assembly ensamblado, string archivo)
    {
        var recurso = ensamblado.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(archivo, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No se encontró la fuente {archivo} en los recursos de Infrastructure.");
        using var stream = ensamblado.GetManifestResourceStream(recurso)!;
        using var memoria = new MemoryStream();
        stream.CopyTo(memoria);
        return memoria.ToArray();
    }
}
