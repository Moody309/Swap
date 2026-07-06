using Swap.Application.Abstractions.Data;
using Swap.Application.Abstractions.Identity;
using Swap.Domain.Entities.Users;
using Swap.Domain.Results;

namespace Swap.Infrastructure.Identity;

internal sealed class UserRegistrationService(
    IIdentityProviderService identityProviderService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IUserRegistrationService
{
    public async Task<Result<Guid>> RegisterUserAsync(
        UserModel userModel,
        CancellationToken cancellationToken)
    {
        Result<string> result = await identityProviderService.RegisterUserAsync(
            userModel, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        var user = User.Create(
            userModel.Email,
            userModel.FirstName,
            userModel.LastName,
            result.Value);

        userRepository.Insert(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
