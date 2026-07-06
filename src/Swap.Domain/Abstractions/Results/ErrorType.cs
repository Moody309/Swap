namespace Swap.Domain.Results;

/// <summary>
/// Represents the category of an application error.
/// </summary>
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    Problem = 2,
    NotFound = 3,
    Conflict = 4
}
