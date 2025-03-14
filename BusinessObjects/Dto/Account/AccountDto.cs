using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Account
{
    public class AccountDto
    {
        public Guid UserId { get; set; }
        public Guid? SkinTypeId { get; set; }
        public string UserName { get; set; }
        public string SurName { get; set; }
        public string LastName { get; set; }
        [EmailAddress]
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string AvatarUrl { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
    }
}
