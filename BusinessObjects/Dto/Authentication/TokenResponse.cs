namespace BusinessObjects.Dto.Authentication;

public class TokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}