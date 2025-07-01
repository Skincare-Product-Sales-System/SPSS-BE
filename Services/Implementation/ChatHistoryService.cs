using AutoMapper;
using BusinessObjects.Dto.ChatHistory;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class ChatHistoryService : IChatHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChatHistoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<ChatHistoryDto>> GetChatHistoryByUserIdAsync(Guid userId, int limit = 100)
        {
            var chatHistory = await _unitOfWork.ChatHistories.GetByUserIdAsync(userId, limit);
            return _mapper.Map<IEnumerable<ChatHistoryDto>>(chatHistory);
        }

        public async Task<IEnumerable<ChatHistoryDto>> GetChatSessionAsync(string sessionId)
        {
            var chatSession = await _unitOfWork.ChatHistories.GetBySessionIdAsync(sessionId);
            return _mapper.Map<IEnumerable<ChatHistoryDto>>(chatSession);
        }

        public async Task<IEnumerable<string>> GetRecentSessionsIdsAsync(Guid userId, int maxSessions = 10)
        {
            var recentSessions = await _unitOfWork.ChatHistories.GetRecentSessionsAsync(userId, maxSessions);
            return recentSessions
                .GroupBy(ch => ch.SessionId)
                .Select(g => g.Key)
                .ToList();
        }

        public async Task<ChatHistoryDto> SaveChatMessageAsync(ChatHistoryForCreationDto chatMessage)
        {
            var chatHistoryEntity = new ChatHistory
            {
                Id = Guid.NewGuid(),
                UserId = chatMessage.UserId,
                MessageContent = chatMessage.MessageContent,
                SenderType = chatMessage.SenderType,
                SessionId = chatMessage.SessionId,
                Timestamp = DateTimeOffset.UtcNow,
                CreatedTime = DateTimeOffset.UtcNow,
                LastUpdatedTime = DateTimeOffset.UtcNow,
                CreatedBy = chatMessage.UserId.ToString(),
                LastUpdatedBy = chatMessage.UserId.ToString(),
                IsDeleted = false
            };

            _unitOfWork.ChatHistories.Add(chatHistoryEntity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ChatHistoryDto>(chatHistoryEntity);
        }
    }
}