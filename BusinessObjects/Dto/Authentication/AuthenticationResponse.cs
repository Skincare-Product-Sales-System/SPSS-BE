namespace BusinessObjects.Dto.Authentication;

public class AuthenticationResponse
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public Guid? RoleId { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}