using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Authentication;

public class LoginRequest
{
    [Required]
    public string UserName { get; set; }
    
    [Required]
    public string Password { get; set; }
}