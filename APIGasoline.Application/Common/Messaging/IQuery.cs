using MediatR;

namespace APIGasoline.Application.Common.Messaging;

/// <summary>
/// Una Query SOLO LEE: nunca guarda cambios, abre transacciones, publica eventos ni crea Commands.
/// Se declara como <c>public sealed record XxxQuery(...) : IQuery&lt;TResultado&gt;</c> en la carpeta Queries
/// y la atiende un único <see cref="IQueryHandler{TQuery,TResponse}"/> llamado XxxQueryHandler.
/// Las pruebas de APIGasoline.Architecture.Tests hacen cumplir estas reglas.
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
