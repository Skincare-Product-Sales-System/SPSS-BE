using AutoMapper;
using BusinessObjects.Dto.Reply;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;
using System;
using System.Collections.Generic;
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

        public async Task<ReplyDto> GetByIdAsync(Guid id)
        {
            var reply = await _unitOfWork.Replies.GetByIdAsync(id);
            if (reply == null)
                throw new KeyNotFoundException($"Reply with ID {id} not found.");

            return _mapper.Map<ReplyDto>(reply);
        }

        public async Task<PagedResponse<ReplyDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var (replies, totalCount) = await _unitOfWork.Replies.GetPagedAsync(pageNumber, pageSize);
            var replyDtos = _mapper.Map<IEnumerable<ReplyDto>>(replies);
            return new PagedResponse<ReplyDto>
            {
                Items = replyDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ReplyDto> CreateAsync(ReplyForCreationDto replyDto)
        {
            if (replyDto == null)
                throw new ArgumentNullException(nameof(replyDto), "Reply data cannot be null.");

            var reply = _mapper.Map<Reply>(replyDto);
            reply.Id = Guid.NewGuid();
            reply.CreatedTime = DateTimeOffset.UtcNow;

            _unitOfWork.Replies.Add(reply);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ReplyDto>(reply);
        }

        public async Task<ReplyDto> UpdateAsync(ReplyForUpdateDto replyDto)
        {
            if (replyDto == null)
                throw new ArgumentNullException(nameof(replyDto), "Reply data cannot be null.");

            var reply = await _unitOfWork.Replies.GetByIdAsync(replyDto.Id);
            if (reply == null)
                throw new KeyNotFoundException($"Reply with ID {replyDto.Id} not found.");

            _mapper.Map(replyDto, reply);
            _unitOfWork.Replies.Update(reply);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ReplyDto>(reply);
        }

        public async Task DeleteAsync(Guid id)
        {
            var reply = await _unitOfWork.Replies.GetByIdAsync(id);
            if (reply == null)
                throw new KeyNotFoundException($"Reply with ID {id} not found.");

            _unitOfWork.Replies.Delete(reply);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
