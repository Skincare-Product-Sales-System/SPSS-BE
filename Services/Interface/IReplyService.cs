using BusinessObjects.Dto.Reply;
using BusinessObjects.Dto.Review;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IReplyService
    {
        Task<ReplyDto> CreateAsync(Guid userId, ReplyForCreationDto replyDto);
        Task<ReplyDto> UpdateAsync(Guid userId, ReplyForUpdateDto replyDto, Guid id);
        Task DeleteAsync(Guid userId, Guid id);
    }
}
