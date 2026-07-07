using Swap.Domain.Results;

namespace Swap.Application.Abstractions.Identity;


/// <summary>
/// Provides operations for interacting with the identity provider.
/// </summary>
public interface IIdentityProviderService
{
    Task<Result<string>> RegisterUserAsync(
        UserModel user,
        CancellationToken cancellationToken = default);

    Task<Result<TokenResponse>> LoginAsync(
    string email,
    string password,
    CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(
    string refreshToken,
    CancellationToken cancellationToken = default);
}
