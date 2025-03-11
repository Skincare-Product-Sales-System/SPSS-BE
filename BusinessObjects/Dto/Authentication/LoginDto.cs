using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Authentication;

public class LoginDto
{
    [EmailAddress]
    public string Email { get; set; }

    public string UserName { get; set; }
    public string Password { get; set; }
}