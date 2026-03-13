using MediatR;

namespace Shared.BuildingBlocks.CQRS;

public interface IQuery<TResponse> : IRequest<TResponse> { }
