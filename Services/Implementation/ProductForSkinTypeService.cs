using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.ProductForSkinType;
using BusinessObjects.Dto.Review;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Services.Interface;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class ProductForSkinTypeService : IProductForSkinTypeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductForSkinTypeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResponse<ProductForSkinTypeDto>> GetProductsBySkinTypeIdAsync(Guid skinTypeId, int pageNumber, int pageSize)
        {
            // Tính toán số bản ghi cần bỏ qua
            var skip = (pageNumber - 1) * pageSize;

            // Truy vấn tổng số sản phẩm có cùng SkinTypeId
            var totalCount = await _unitOfWork.ProductForSkinTypes.Entities
                .Where(pfs => pfs.SkinTypeId == skinTypeId)
                .CountAsync();

            // Lấy danh sách sản phẩm theo SkinTypeId với phân trang
            var productForSkinTypes = await _unitOfWork.ProductForSkinTypes.Entities
                .Include(pfs => pfs.Product) // Bao gồm thông tin Product
                    .ThenInclude(p => p.ProductImages) // Bao gồm hình ảnh sản phẩm
                .Where(pfs => pfs.SkinTypeId == skinTypeId)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            // Nhóm sản phẩm theo SkinTypeId và ánh xạ sang DTO
            var result = productForSkinTypes
                .GroupBy(pfs => pfs.SkinTypeId)
                .Select(group => new ProductForSkinTypeDto
                {
                    SkinTypeId = group.Key,
                    Products = group.Select(pfs => new ProductDto
                    {
                        Id = pfs.Product.Id,
                        Name = pfs.Product.Name,
                        Description = pfs.Product.Description,
                        Price = pfs.Product.Price,
                        MarketPrice = pfs.Product.MarketPrice,
                        Thumbnail = pfs.Product.ProductImages.FirstOrDefault(img => img.IsThumbnail)?.ImageUrl ?? string.Empty
                    }).ToList()
                }).ToList();

            // Trả về kết quả phân trang dưới dạng PagedResponse
            return new PagedResponse<ProductForSkinTypeDto>
            {
                Items = result,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

    }
}
