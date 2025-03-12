using AutoMapper;
using BusinessObjects.Dto.CartItem;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<CartItemDto>> GetByUserIdAsync(Guid userId)
        {
            // Retrieve cart items associated with the user ID
            var cartItems = await _unitOfWork.CartItems.Entities
                .Include(ci => ci.ProductItem)
                    .ThenInclude(p => p.Product)
                        .ThenInclude(c => c.ProductCategory)
                .Include(ci => ci.ProductItem)
                    .ThenInclude(p => p.Product)
                        .ThenInclude(c => c.Brand)
                .Include(ci => ci.ProductItem)
                    .ThenInclude(p => p.Product)
                        .ThenInclude(c => c.ProductImages)
                .Include(ci => ci.ProductItem)
                    .ThenInclude(p => p.ProductConfigurations)
                        .ThenInclude(pi => pi.VariationOption)
                .Where(ci => ci.UserId == userId && !ci.IsDeleted)
                .OrderByDescending(ci => ci.LastUpdatedTime)
                .ToListAsync();

            // Map to DTOs using AutoMapper
            return _mapper.Map<IEnumerable<CartItemDto>>(cartItems);
        }

        public async Task<CartItemDto> GetByIdAsync(Guid id)
        {
            var cartItem = await _unitOfWork.CartItems.GetByIdAsync(id);
            if (cartItem == null)
                throw new KeyNotFoundException($"CartItem with ID {id} not found.");

            return _mapper.Map<CartItemDto>(cartItem);
        }

        public async Task<bool> CreateAsync(CartItemForCreationDto cartItemDto, Guid userId)
        {
            if (cartItemDto == null)
                throw new ArgumentNullException(nameof(cartItemDto), "CartItem data cannot be null.");

            // Kiểm tra xem người dùng đã có CartItem với cùng ProductId chưa
            var existingCartItem = await _unitOfWork.CartItems
                .SingleOrDefaultAsync(c => c.UserId == userId && c.ProductItemId == cartItemDto.ProductItemId && !c.IsDeleted);

            if (existingCartItem != null)
            {
                // Nếu đã tồn tại, tăng số lượng
                existingCartItem.Quantity += cartItemDto.Quantity;

                _unitOfWork.CartItems.Update(existingCartItem);
                await _unitOfWork.SaveChangesAsync();

                _mapper.Map<CartItemDto>(existingCartItem);
                return true;
            }

            // Nếu chưa tồn tại, tạo mới CartItem
            var cartItem = _mapper.Map<CartItem>(cartItemDto);
            cartItem.Id = Guid.NewGuid();
            cartItem.CreatedTime = DateTimeOffset.UtcNow;
            cartItem.LastUpdatedTime = DateTimeOffset.UtcNow;
            cartItem.UserId = userId;

            _unitOfWork.CartItems.Add(cartItem);
            await _unitOfWork.SaveChangesAsync();

            _mapper.Map<CartItemDto>(cartItem);
            return true;
        }

        public async Task<CartItemDto> UpdateAsync(Guid id, CartItemForUpdateDto cartItemDto)
        {
            if (cartItemDto == null)
                throw new ArgumentNullException(nameof(cartItemDto), "CartItem data cannot be null.");

            var cartItem = await _unitOfWork.CartItems.GetByIdAsync(id);
            if (cartItem == null)
                throw new KeyNotFoundException($"CartItem with ID {id} not found.");
            cartItem.LastUpdatedTime = DateTimeOffset.UtcNow;
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
