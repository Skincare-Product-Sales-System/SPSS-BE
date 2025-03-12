using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Authentication;

public class LoginRequest
{
    [Required]
    public string UsernameOrEmail { get; set; }
    
    [Required]
    public string Password { get; set; }
}