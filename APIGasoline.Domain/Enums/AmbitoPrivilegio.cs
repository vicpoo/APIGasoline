namespace APIGasoline.Domain.Enums;

/// <summary>
/// A qué roles se puede asignar un privilegio.
/// Plataforma = solo a roles globales (ej. empresa.crear).
/// Empresa = a cualquier rol (global o exclusivo de una empresa).
/// </summary>
public enum AmbitoPrivilegio
{
    Plataforma = 0,
    Empresa = 1
}
