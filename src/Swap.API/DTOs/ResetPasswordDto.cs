namespace Swap.API.DTOs;

public class ResetPasswordDto
{
    public string PhoneNumber { get; set; }
    public string Token { get; set; }
    public string NewPassword { get; set; }
}
