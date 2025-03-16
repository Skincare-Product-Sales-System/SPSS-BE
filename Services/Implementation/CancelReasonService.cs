using AutoMapper;
using BusinessObjects.Dto.CancelReason;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation
{
    public class CancelReasonService : ICancelReasonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly string _currentUser; // Placeholder for current user, ideally injected

        public CancelReasonService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _currentUser = "System"; // Replace with actual user retrieval (e.g., from IHttpContextAccessor or a service)
        }

        public async Task<CancelReasonDto> GetByIdAsync(Guid id)
        {
            var cancelReason = await _unitOfWork.CancelReasons.GetByIdAsync(id);
            if (cancelReason == null)
                throw new KeyNotFoundException($"Cancel Reason with ID {id} not found or has been deleted.");

            return _mapper.Map<CancelReasonDto>(cancelReason);
        }

        public async Task<PagedResponse<CancelReasonDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var (cancelReasons, totalCount) = await _unitOfWork.CancelReasons.GetPagedAsync(
                pageNumber,
                pageSize,
                null
            );
            var cancelReasonDtos = _mapper.Map<IEnumerable<CancelReasonDto>>(cancelReasons);
            return new PagedResponse<CancelReasonDto>
            {
                Items = cancelReasonDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<CancelReasonDto> CreateAsync(CancelReasonForCreationDto cancelReasonDto, Guid userId)
        {
            if (cancelReasonDto == null)
                throw new ArgumentNullException(nameof(cancelReasonDto), "Cancel reason data cannot be null.");

            var cancelReason = _mapper.Map<CancelReason>(cancelReasonDto);

            _unitOfWork.CancelReasons.Add(cancelReason);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CancelReasonDto>(cancelReason);
        }

        public async Task<CancelReasonDto> UpdateAsync(Guid id, CancelReasonForUpdateDto cancelReasonDto, Guid userId)
        {
            if (cancelReasonDto == null)
                throw new ArgumentNullException(nameof(cancelReasonDto), "Cancel reason data cannot be null.");

            var cancelReason = await _unitOfWork.CancelReasons.GetByIdAsync(id);
            if (cancelReason == null)
                throw new KeyNotFoundException($"Cancel reason with ID {id} not found or has been deleted.");

            _mapper.Map(cancelReasonDto, cancelReason);
            _unitOfWork.CancelReasons.Update(cancelReason);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CancelReasonDto>(cancelReason);
        }

        public async Task DeleteAsync(Guid id, Guid userId)
        {
            var cancelReason = await _unitOfWork.CancelReasons.GetByIdAsync(id);
            if (cancelReason == null)
                throw new KeyNotFoundException($"Cancel reason with ID {id} not found or has been deleted.");

            _unitOfWork.CancelReasons.Update(cancelReason); // Soft delete via update
            await _unitOfWork.SaveChangesAsync();
        }
    }
}