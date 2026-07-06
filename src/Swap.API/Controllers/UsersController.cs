using Microsoft.AspNetCore.Mvc;
using Swap.Application.Abstractions.Identity;
using Swap.Domain.Results;

namespace Swap.API.Controllers;

[Route("users")]
[ApiController]
public class UsersController(IUserRegistrationService registrationService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var userModel = new UserModel(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        Result<Guid> result = await registrationService.RegisterUserAsync(userModel, cancellationToken);

        return result.IsSuccess
            ? Ok(new { UserId = result.Value })
            : BadRequest(result.Error);
    }
}
