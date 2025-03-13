using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Authentication;

public class LogoutRequest
{
    [Required] public string RefreshToken { get; set; }
}