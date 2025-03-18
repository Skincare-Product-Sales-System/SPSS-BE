using BusinessObjects.Dto.Account;
using Microsoft.AspNetCore.Http;

namespace Services.Interface
{
    public interface IAccountService
    {
        Task<AccountDto> GetAccountInfoAsync(Guid userId);
        Task<AccountDto> UpdateAccountInfoAsync(Guid userId, AccountForUpdateDto accountUpdateDto);
        Task<string> UpdateAvatarAsync(Guid userId, IFormFile avatarFile);
        Task<bool> DeleteFirebaseLink(string imageUrl);
    }
}
