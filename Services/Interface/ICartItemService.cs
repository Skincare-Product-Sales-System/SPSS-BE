using BusinessObjects.Dto.CartItem;
using BusinessObjects.Dto.Reply;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface ICartItemService
    {
        Task<CartItemDto> GetByIdAsync(Guid id);
        Task<PagedResponse<CartItemDto>> GetPagedAsync(int pageNumber, int pageSize);
        Task<CartItemDto> CreateAsync(CartItemForCreationDto reviewDto);
        Task<CartItemDto> UpdateAsync(CartItemForUpdateDto reviewDto);
        Task DeleteAsync(Guid id);
    }
}
