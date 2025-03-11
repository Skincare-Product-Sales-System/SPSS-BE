using AutoMapper;
using BusinessObjects.Dto.CancelReason;
using BusinessObjects.Dto.ProductStatus;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation
{
    public class ProductStatusService : IProductStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductStatusService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Guid?> GetFirstAvailableProductStatusIdAsync()
        {
            var productStatuses = await _unitOfWork.ProductStatuses.FindAsync(ps => !ps.IsDeleted && !string.IsNullOrEmpty(ps.StatusName));
            return productStatuses.Select(ps => ps.Id).FirstOrDefault();
        }

        public async Task<ProductStatusDto> GetByIdAsync(Guid id)
        {
            var productStatus = await _unitOfWork.ProductStatuses.GetByIdAsync(id);
            if (productStatus == null)
                throw new KeyNotFoundException($"ProductStatus with ID {id} not found.");

            return _mapper.Map<ProductStatusDto>(productStatus);
        }

        public async Task<PagedResponse<ProductStatusDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var (productStatuses, totalCount) = await _unitOfWork.ProductStatuses.GetPagedAsync(pageNumber, pageSize);
            var productStatusDtos = _mapper.Map<IEnumerable<ProductStatusDto>>(productStatuses);
            return new PagedResponse<ProductStatusDto>
            {
                Items = productStatusDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ProductStatusDto> CreateAsync(ProductStatusForCreationDto productStatusDto)
        {
            if (productStatusDto == null)
                throw new ArgumentNullException(nameof(productStatusDto), "Data cannot be null.");
            var productStatus = _mapper.Map<ProductStatus>(productStatusDto);
            _unitOfWork.ProductStatuses.Add(productStatus);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ProductStatusDto>(productStatus);
        }

        public async Task<ProductStatusDto> UpdateAsync(Guid id, ProductStatusForUpdateDto productStatusDto)
        {
            if (productStatusDto == null)
                throw new ArgumentNullException(nameof(productStatusDto), "Data cannot be null.");
            var productStatus = await _unitOfWork.ProductStatuses.GetByIdAsync(id);
            if (productStatus == null)
                throw new KeyNotFoundException($"Cancel reason with ID {id} not found.");
            _mapper.Map(productStatusDto, productStatus);
            _unitOfWork.ProductStatuses.Update(productStatus);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ProductStatusDto>(productStatus);
        }

        public async Task DeleteAsync(Guid id)
        {
            var productStatus = await _unitOfWork.ProductStatuses.GetByIdAsync(id);
            if (productStatus == null)
                throw new KeyNotFoundException($"Product status with ID {id} not found.");

            productStatus.IsDeleted = true;
            productStatus.DeletedTime = DateTimeOffset.UtcNow;

            _unitOfWork.ProductStatuses.Update(productStatus);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
