namespace Swap.API.Controllers;

public sealed record RegisterUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);

