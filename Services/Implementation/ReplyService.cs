using AutoMapper;
using BusinessObjects.Dto.Reply;
using BusinessObjects.Models;
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

        public async Task<ReplyDto> CreateAsync(Guid userId, ReplyForUpdateDto replyDto)
        {
            if (replyDto == null)
                throw new ArgumentNullException(nameof(replyDto), "Reply data cannot be null.");

            var reply = _mapper.Map<Reply>(replyDto);
            reply.Id = Guid.NewGuid();
            reply.CreatedTime = DateTimeOffset.UtcNow;
            reply.UserId = userId;
            reply.CreatedBy = userId.ToString();
            reply.LastUpdatedTime = DateTimeOffset.UtcNow;
            reply.LastUpdatedBy = userId.ToString();

            _unitOfWork.Replies.Add(reply);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ReplyDto>(reply);
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
