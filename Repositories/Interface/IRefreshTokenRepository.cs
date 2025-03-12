using BusinessObjects.Models;

namespace Repositories.Interface;

public interface IRefreshTokenRepository : IRepositoryBase<RefreshToken, Guid>
{
    Task<RefreshToken> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId);
}