using Swap.Domain.Results;

namespace Swap.Application.Abstractions.Identity;

/// <summary>
/// Identity provider related errors.
/// </summary>
public static class IdentityProviderErrors
{
    public static readonly Error EmailIsNotUnique = Error.Conflict(
        code: "EmailIsNotUnique",
        description: "The provided email is not unique.");
}
