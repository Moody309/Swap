using System.ComponentModel.DataAnnotations;
using Swap.API.Services;

namespace Swap.API.Database;

public class Users
{
    [Key]
    public int Id { get; set; } 

    [Required]
    [StringLength(50)] 
    public string UserName { get; set; } = string.Empty; 
    [Required]
    [Phone] 
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
