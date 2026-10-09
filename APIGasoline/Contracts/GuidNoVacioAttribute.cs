using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class GuidNoVacioAttribute : ValidationAttribute
{
    public GuidNoVacioAttribute() : base("El campo {0} es obligatorio y debe ser un UUID válido.")
    {
    }

    public override bool IsValid(object? value) => value is not Guid id || id != Guid.Empty;
}
