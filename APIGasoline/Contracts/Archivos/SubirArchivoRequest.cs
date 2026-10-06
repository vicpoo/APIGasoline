using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Archivos;

/// <summary>
/// Cuerpo multipart/form-data con un solo archivo en el campo "archivo".
/// </summary>
public class SubirArchivoRequest
{
    [Required]
    public IFormFile? Archivo { get; set; }
}
