using AutoMapper;
using BusinessObjects.Dto.Product;
using BusinessObjects.Dto.ProductCategory;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ProductCategoryDto> GetByIdAsync(Guid id)
        {
            var category = await _unitOfWork.ProductCategories.GetByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Product Category with ID {id} not found.");

            return _mapper.Map<ProductCategoryDto>(category);
        }

        public async Task<PagedResponse<ProductCategoryDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var (categories, totalCount) = await _unitOfWork.ProductCategories.GetPagedAsync(
                pageNumber,
                pageSize,
                cr => cr.IsDeleted == false // Filter out deleted cancel reasons
            );
            var categoryDtos = _mapper.Map<IEnumerable<ProductCategoryDto>>(categories);
            return new PagedResponse<ProductCategoryDto>
            {
                Items = categoryDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ProductCategoryDto> CreateAsync(ProductCategoryForCreationDto categoryDto)
        {
            if (categoryDto == null)
                throw new ArgumentNullException(nameof(categoryDto), "Product Category data cannot be null.");
            var category = _mapper.Map<ProductCategory>(categoryDto);
            _unitOfWork.ProductCategories.Add(category);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ProductCategoryDto>(category);
        }

        public async Task<ProductCategoryDto> UpdateAsync(ProductCategoryForUpdateDto categoryDto)
        {
            if (categoryDto == null)
                throw new ArgumentNullException(nameof(categoryDto), "Product Category data cannot be null.");
            var category = await _unitOfWork.ProductCategories.GetByIdAsync(categoryDto.Id);
            if (category == null)
                throw new KeyNotFoundException($"Product Category with ID {categoryDto.Id} not found.");
            _mapper.Map(categoryDto, category);
            _unitOfWork.ProductCategories.Update(category);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ProductCategoryDto>(category);
        }

        public async Task DeleteAsync(Guid id)
        {
            var category = await _unitOfWork.ProductCategories.GetByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Product Category with ID {id} not found.");
            _unitOfWork.ProductCategories.Delete(category);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
