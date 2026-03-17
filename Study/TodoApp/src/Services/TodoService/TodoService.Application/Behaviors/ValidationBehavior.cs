using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.Result;

namespace TodoService.Application.Behaviors;

/// <summary>
/// Pipeline behavior that runs FluentValidation before the handler.
/// Returns a failure Result instead of throwing, keeping the error flow consistent.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0) return await next(cancellationToken);

        logger.LogWarning("Validation failed for {Request}: {Errors}",
            typeof(TRequest).Name,
            string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));

        // Return a failure Result instead of throwing
        var errors = string.Join("; ", failures.Select(f => f.ErrorMessage));
        var error = Error.Validation("Validation.Failed", errors);

        // Use dynamic dispatch to create the correct Result<T> type
        if (TryCreateFailureResult<TResponse>(error, out var failResult))
            return failResult!;

        throw new ValidationException(failures);
    }

    private static bool TryCreateFailureResult<T>(Error error, out T? result)
    {
        var responseType = typeof(T);

        if (responseType == typeof(Result))
        {
            result = (T)(object)Result.Failure(error);
            return true;
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failMethod = responseType.GetMethod("Failure")!;
            result = (T)failMethod.Invoke(null, [error])!;
            return true;
        }

        result = default;
        return false;
    }
}
