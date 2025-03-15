namespace BusinessObjects.Dto.Authentication;

public class RegisterRequest
{
    public string UserName { get; set; } 
    public string SurName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string PhoneNumber { get; set; } 
    public string Password { get; set; } 

}