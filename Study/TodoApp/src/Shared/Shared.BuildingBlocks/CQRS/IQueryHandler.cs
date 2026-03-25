using MediatR;
using Shared.BuildingBlocks.Result;

namespace Shared.BuildingBlocks.CQRS;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse> { }
