using APIGasoline.Application.Common.Exceptions;

namespace APIGasoline.Application.Common.Security;

/// <summary>
/// Reglas mínimas para una contraseña nueva.
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 8;
    public const int MaxLength = 100;

    public static void Validar(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new BadRequestException("La contraseña es obligatoria.");

        if (password.Length < MinLength || password.Length > MaxLength)
            throw new BadRequestException($"La contraseña debe tener entre {MinLength} y {MaxLength} caracteres.");

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            throw new BadRequestException("La contraseña debe contener al menos una letra y un número.");
    }
}
