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
        Task<ReplyDto> GetByIdAsync(Guid id);
        Task<PagedResponse<ReplyDto>> GetPagedAsync(int pageNumber, int pageSize);
        Task<ReplyDto> CreateAsync(ReplyForCreationDto reviewDto);
        Task<ReplyDto> UpdateAsync(ReplyForUpdateDto reviewDto);
        Task DeleteAsync(Guid id);
    }
}
