using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Account
{
    public class AccountForUpdateDto
    {
        public string UserName { get; set; }
        public string SurName { get; set; }
        public string LastName { get; set; }
        [EmailAddress]
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
    }
}
