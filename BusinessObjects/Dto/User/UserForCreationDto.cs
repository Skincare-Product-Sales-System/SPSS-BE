using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.User;

public class UserForCreationDto
{
    public Guid? SkinTypeId { get; set; }
    public Guid? RoleId { get; set; }
    public string SurName { get; set; }
    public string LastName { get; set; }
    [EmailAddress]
    public string EmailAddress { get; set; }
    public string PhoneNumber { get; set; }
    public string Status { get; set; }
    public string Password { get; set; }
    public string AvatarUrl { get; set; }

}