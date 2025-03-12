using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Authentication;

public class RefreshTokenRequest
{
    
    [Required]
    public string AccessToken { get; set; }
    
    [Required]
    public string RefreshToken { get; set; }
}