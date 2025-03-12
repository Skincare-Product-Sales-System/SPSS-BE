using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObjects.Models;

public class UserDetail
{
    [Key]
    public string UserId { get; set; } // FK + PK to match IdentityUser Id (string)

    public Guid? SkinTypeId { get; set; }
    public string SurName { get; set; }
    public string LastName { get; set; }
    public string Status { get; set; }
    public string AvatarUrl { get; set; }

    // Audit fields for UserDetail
    public string CreatedBy { get; set; }
    public string LastUpdatedBy { get; set; }
    public string DeletedBy { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset? LastUpdatedTime { get; set; }
    public DateTimeOffset? DeletedTime { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public virtual SkinType? SkinType { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }
}