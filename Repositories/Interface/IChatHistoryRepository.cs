using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IChatHistoryRepository : IRepositoryBase<ChatHistory, Guid>
    {
        Task<IEnumerable<ChatHistory>> GetByUserIdAsync(Guid userId, int limit = 100);
        Task<IEnumerable<ChatHistory>> GetBySessionIdAsync(string sessionId);
        Task<IEnumerable<ChatHistory>> GetRecentSessionsAsync(Guid userId, int maxSessions = 10);
        Task<IEnumerable<ChatHistory>> GetByUserIdAndSessionIdAsync(Guid userId, string sessionId);
    }
}