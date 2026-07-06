using Swap.Domain.Results;

namespace Swap.Application.Abstractions.Identity;

public interface IUserRegistrationService
{
    Task<Result<Guid>> RegisterUserAsync(
   UserModel userModel,
   CancellationToken cancellationToken);
}
