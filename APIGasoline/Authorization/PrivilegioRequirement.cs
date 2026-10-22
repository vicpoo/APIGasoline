using Microsoft.AspNetCore.Authorization;

namespace APIGasoline.Authorization;

/// <summary>Se cumple si el usuario tiene AL MENOS UNO de los privilegios indicados.</summary>
public sealed class PrivilegioRequirement : IAuthorizationRequirement
{
    public PrivilegioRequirement(IReadOnlyList<string> claves)
    {
        Claves = claves;
    }

    public IReadOnlyList<string> Claves { get; }

    public override string ToString() => $"Privilegio: {string.Join(" o ", Claves)}";
}
