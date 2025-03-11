using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.User;

public class UserDto
{
    public int UserId { get; set; }
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

    public string CreatedBy { get; set; }

    public string LastUpdatedBy { get; set; }

    public string DeletedBy { get; set; }

    public DateTimeOffset CreatedTime { get; set; }

    public DateTimeOffset? LastUpdatedTime { get; set; }

    public DateTimeOffset? DeletedTime { get; set; }

    public bool IsDeleted { get; set; }

}