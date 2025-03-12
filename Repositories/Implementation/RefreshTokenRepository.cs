using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;

namespace Repositories.Implementation;

public class RefreshTokenRepository : RepositoryBase<RefreshToken, Guid>, IRefreshTokenRepository
{
    public RefreshTokenRepository(SPSSContext context) : base(context)
    {
    }

    public async Task<RefreshToken> GetByTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(u => u.Token == token);

        return refreshToken;
    }

    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId)
    {
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync();

        return refreshTokens;
    }
}