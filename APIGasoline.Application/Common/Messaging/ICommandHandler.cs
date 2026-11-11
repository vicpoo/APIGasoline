using MediatR;

namespace APIGasoline.Application.Common.Messaging;

/// <summary>Atiende un <see cref="ICommand"/> sin resultado. La clase debe ser <c>sealed</c>.</summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
{
}

/// <summary>Atiende un <see cref="ICommand{TResponse}"/>. La clase debe ser <c>sealed</c>.</summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}
