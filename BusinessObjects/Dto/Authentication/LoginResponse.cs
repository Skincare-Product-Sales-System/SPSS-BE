namespace BusinessObjects.Dto.Authentication;

public class LoginResponse
{
    public string Id { get; set; }
    public string Token { get; set; }
    public string Role { get; set; }
    public string RefreshToken { get; set; }
}