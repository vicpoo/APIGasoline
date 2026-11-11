using MediatR;

namespace APIGasoline.Application.Common.Messaging;

/// <summary>Atiende un <see cref="IQuery{TResponse}"/>. La clase debe ser <c>sealed</c> y solo leer.</summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
