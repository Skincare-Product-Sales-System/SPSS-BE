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
        public async Task<PagedResponse<CartItemDto>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            // Tính toán số bản ghi cần bỏ qua
            var skip = (pageNumber - 1) * pageSize;

            // Truy vấn tổng số bản ghi
            var totalCount = await _unitOfWork.CartItems.Entities
                .Where(ci => ci.UserId == userId)
                .CountAsync();

            // Truy vấn dữ liệu với phân trang
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
                .Where(ci => ci.UserId == userId)
                .Skip(skip) // Bỏ qua số lượng phần tử theo trang
                .Take(pageSize) // Lấy số lượng phần tử theo kích thước trang
                .ToListAsync();

            // Ánh xạ sang DTOs bằng AutoMapper
            var mappedItems = _mapper.Map<IEnumerable<CartItemDto>>(cartItems);

            // Tạo đối tượng PagedResponse
            return new PagedResponse<CartItemDto>
            {
                Items = mappedItems,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> CreateAsync(CartItemForCreationDto cartItemDto, Guid userId)
        {
            if (cartItemDto == null)
                throw new ArgumentNullException(nameof(cartItemDto), "CartItem data cannot be null.");

            // Fetch the ProductItem to check its stock
            var productItem = await _unitOfWork.ProductItems
                .SingleOrDefaultAsync(p => p.Id == cartItemDto.ProductItemId);

            if (productItem == null)
                throw new KeyNotFoundException($"ProductItem with ID {cartItemDto.ProductItemId} not found.");

            if (cartItemDto.Quantity > productItem.QuantityInStock)
                throw new InvalidOperationException("Requested quantity exceeds available stock.");

            // Check if the user already has a CartItem with the same ProductItemId
            var existingCartItem = await _unitOfWork.CartItems
                .SingleOrDefaultAsync(c => c.UserId == userId && c.ProductItemId == cartItemDto.ProductItemId);

            if (existingCartItem != null)
            {
                // Check if the updated quantity exceeds stock
                if (existingCartItem.Quantity + cartItemDto.Quantity > productItem.QuantityInStock)
                    throw new InvalidOperationException("Updated quantity exceeds available stock.");

                // Update the existing CartItem's quantity
                existingCartItem.Quantity += cartItemDto.Quantity;

                _unitOfWork.CartItems.Update(existingCartItem);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }

            // Create a new CartItem if it doesn't exist
            var cartItem = _mapper.Map<CartItem>(cartItemDto);
            cartItem.Id = Guid.NewGuid();
            cartItem.UserId = userId;

            _unitOfWork.CartItems.Add(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<CartItemDto> UpdateAsync(Guid id, CartItemForUpdateDto cartItemDto)
        {
            if (cartItemDto == null)
                throw new ArgumentNullException(nameof(cartItemDto), "CartItem data cannot be null.");

            var cartItem = await _unitOfWork.CartItems.GetByIdAsync(id);
            if (cartItem == null)
                throw new KeyNotFoundException($"CartItem with ID {id} not found.");
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
