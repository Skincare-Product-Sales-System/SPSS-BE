using AutoMapper;
using BusinessObjects.Dto.Reply;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Services.Interface;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class ReplyService : IReplyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReplyService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ReplyDto> CreateAsync(Guid userId, ReplyForCreationDto replyDto)
        {
            if (replyDto == null)
                throw new ArgumentNullException(nameof(replyDto), "Reply data cannot be null.");

            // Check if the reviewId exists
            var reviewExists = await _unitOfWork.Reviews.Entities.AnyAsync(r => r.Id == replyDto.ReviewId);
            if (!reviewExists)
                throw new ArgumentException("The specified reviewId does not exist.", nameof(replyDto.ReviewId));

            // Manual mapping of Reply entity
            var reply = new Reply
            {
                Id = Guid.NewGuid(),
                ReviewId = replyDto.ReviewId,
                ReplyContent = replyDto.ReplyContent,
                CreatedTime = DateTimeOffset.UtcNow,
                UserId = userId,
                CreatedBy = userId.ToString(),
                LastUpdatedTime = DateTimeOffset.UtcNow,
                LastUpdatedBy = userId.ToString(),
                IsDeleted = false
            };

            // Add the reply to the database
            _unitOfWork.Replies.Add(reply);
            await _unitOfWork.SaveChangesAsync();

            // Manual mapping of ReplyDto for return
            var replyDtoResult = new ReplyDto
            {
                Id = reply.Id,
                ReplyContent = reply.ReplyContent,
            };

            return replyDtoResult;
        }

        public async Task<ReplyDto> UpdateAsync(Guid userId, ReplyForUpdateDto replyDto, Guid id)
        {
            if (replyDto == null)
                throw new ArgumentNullException(nameof(replyDto), "Reply data cannot be null.");

            var reply = await _unitOfWork.Replies.GetByIdAsync(id);
            if (reply == null)
                throw new KeyNotFoundException($"Reply with ID {id} not found.");

            _mapper.Map(replyDto, reply);
            reply.LastUpdatedTime = DateTimeOffset.UtcNow;
            reply.LastUpdatedBy = userId.ToString();
            _unitOfWork.Replies.Update(reply);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ReplyDto>(reply);
        }

        public async Task DeleteAsync(Guid userId, Guid id)
        {
            var reply = await _unitOfWork.Replies.GetByIdAsync(id);
            if (reply == null)
                throw new KeyNotFoundException($"Reply with ID {id} not found.");
            reply.IsDeleted = true;
            reply.DeletedTime = DateTimeOffset.UtcNow;
            reply.DeletedBy = userId.ToString();

            _unitOfWork.Replies.Update(reply);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
