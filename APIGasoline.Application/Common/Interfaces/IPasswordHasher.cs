namespace APIGasoline.Application.Common.Interfaces;

/// <summary>
/// Hash de contraseñas. La implementación vive en Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
