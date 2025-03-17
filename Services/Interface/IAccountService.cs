using BusinessObjects.Dto.Account;

namespace Services.Interface
{
    public interface IAccountService
    {
        Task<AccountDto> GetAccountInfoAsync(Guid userId);
        Task<AccountDto> UpdateAccountInfoAsync(Guid userId, AccountForUpdateDto accountUpdateDto);
    }
}
