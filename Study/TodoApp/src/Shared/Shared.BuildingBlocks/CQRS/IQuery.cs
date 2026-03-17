using MediatR;
using Shared.BuildingBlocks.Result;

namespace Shared.BuildingBlocks.CQRS;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
