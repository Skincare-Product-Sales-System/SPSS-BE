using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Authentication;

public class LoginRequest
{
    public string UserName { get; set; }
        
    [EmailAddress]
    public string Email { get; set; }
        
    [Required]
    public string Password { get; set; }
}