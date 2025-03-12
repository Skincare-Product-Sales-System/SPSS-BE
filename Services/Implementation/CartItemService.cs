using AutoMapper;
using BusinessObjects.Dto.CartItem;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class CartItemService : ICartItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartItemService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<CartItemDto> GetByIdAsync(Guid id)
        {
            var cartItem = await _unitOfWork.CartItems.GetByIdAsync(id);
            if (cartItem == null)
                throw new KeyNotFoundException($"CartItem with ID {id} not found.");

            return _mapper.Map<CartItemDto>(cartItem);
        }

        public async Task<PagedResponse<CartItemDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var (cartItems, totalCount) = await _unitOfWork.CartItems.GetPagedAsync(
                pageNumber,
                pageSize,
                cr => cr.IsDeleted == false // Filter out deleted cancel reasons
            );
            var cartItemDtos = _mapper.Map<IEnumerable<CartItemDto>>(cartItems);
            return new PagedResponse<CartItemDto>
            {
                Items = cartItemDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<CartItemDto> CreateAsync(CartItemForCreationDto cartItemDto)
        {
            if (cartItemDto == null)
                throw new ArgumentNullException(nameof(cartItemDto), "CartItem data cannot be null.");

            var cartItem = _mapper.Map<CartItem>(cartItemDto);
            cartItem.Id = Guid.NewGuid();
            cartItem.CreatedTime = DateTimeOffset.UtcNow;

            _unitOfWork.CartItems.Add(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CartItemDto>(cartItem);
        }

        public async Task<CartItemDto> UpdateAsync(CartItemForUpdateDto cartItemDto)
        {
            if (cartItemDto == null)
                throw new ArgumentNullException(nameof(cartItemDto), "CartItem data cannot be null.");

            var cartItem = await _unitOfWork.CartItems.GetByIdAsync(cartItemDto.Id);
            if (cartItem == null)
                throw new KeyNotFoundException($"CartItem with ID {cartItemDto.Id} not found.");

            _mapper.Map(cartItemDto, cartItem);
            _unitOfWork.CartItems.Update(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CartItemDto>(cartItem);
        }

        public async Task DeleteAsync(Guid id)
        {
            var cartItem = await _unitOfWork.CartItems.GetByIdAsync(id);
            if (cartItem == null)
                throw new KeyNotFoundException($"CartItem with ID {id} not found.");

            _unitOfWork.CartItems.Delete(cartItem);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
