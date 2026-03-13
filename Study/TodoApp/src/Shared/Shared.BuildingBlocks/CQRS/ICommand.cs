using MediatR;

namespace Shared.BuildingBlocks.CQRS;

public interface ICommand : IRequest { }

public interface ICommand<TResponse> : IRequest<TResponse> { }
