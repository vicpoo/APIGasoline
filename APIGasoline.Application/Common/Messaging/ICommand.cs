using MediatR;

namespace APIGasoline.Application.Common.Messaging;

/// <summary>
/// Un Command CAMBIA el estado del sistema (crea, edita, borra, aprueba...).
/// Se declara como <c>public sealed record XxxCommand(...) : ICommand</c> en la carpeta Commands
/// y lo atiende un único <see cref="ICommandHandler{TCommand}"/> llamado XxxCommandHandler.
/// Las pruebas de APIGasoline.Architecture.Tests hacen cumplir estas reglas.
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>Command que además devuelve un resultado (normalmente el DTO del registro afectado).</summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
