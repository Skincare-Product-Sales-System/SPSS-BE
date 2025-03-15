namespace BusinessObjects.Dto.Authentication;

public class AuthUserDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string EmailAddress { get; set; }
    public string AvatarUrl { get; set; }
    public string Role { get; set; }
}