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
        Task<IEnumerable<CartItemDto>> GetByUserIdAsync(Guid userId);
        Task<bool> CreateAsync(CartItemForCreationDto reviewDto, Guid userId);
        Task<CartItemDto> UpdateAsync(Guid id, CartItemForUpdateDto reviewDto);
        Task DeleteAsync(Guid id);
    }
}
