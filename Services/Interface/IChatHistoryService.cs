using BusinessObjects.Dto.ChatHistory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IChatHistoryService
    {
        Task<IEnumerable<ChatHistoryDto>> GetChatHistoryByUserIdAsync(Guid userId, int limit = 100);
        Task<IEnumerable<ChatHistoryDto>> GetChatSessionAsync(string sessionId);
        Task<IEnumerable<string>> GetRecentSessionsIdsAsync(Guid userId, int maxSessions = 10);
        Task<ChatHistoryDto> SaveChatMessageAsync(ChatHistoryForCreationDto chatMessage);
    }
}