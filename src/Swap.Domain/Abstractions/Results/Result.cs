using System.Diagnostics.CodeAnalysis;

namespace Swap.Domain.Results;

/// <summary>
/// Represents the outcome of an operation.
/// </summary>
public class Result
{
    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with a failed operation.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result with a value type.
    /// </summary>
    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);
}

/// <summary>
/// Represents the outcome of an operation that returns a value.
/// </summary>
public class Result<TValue> : Result
{
    public Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the operation result value.
    /// Throws if the operation failed.
    /// </summary>
    [NotNull]
    [AllowNull]
    public TValue Value => IsSuccess
        ? field!
        : throw new InvalidOperationException(
            "The value of a failure result can't be accessed.");

    /// <summary>
    /// Converts a value into a successful result.
    /// A null value produces a failure result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null
            ? Success(value)
            : Failure<TValue>(Error.NullValue);

    /// <summary>
    /// Creates a validation failure result.
    /// </summary>
    public static Result<TValue> ValidationFailure(Error error) =>
        new(default, false, error);
}
