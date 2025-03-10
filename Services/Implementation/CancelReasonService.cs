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

        public CancelReasonService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CancelReasonDto> GetByIdAsync(Guid id)
        {
            var cancelReason = await _unitOfWork.CancelReasons.GetByIdAsync(id);
            if (cancelReason == null)
                throw new KeyNotFoundException($"Cancel Reason with ID {id} not found.");

            return _mapper.Map<CancelReasonDto>(cancelReason);
        }

        public async Task<PagedResponse<CancelReasonDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var (cancelReasons, totalCount) = await _unitOfWork.CancelReasons.GetPagedAsync(pageNumber, pageSize);
            var cancleReasonDtos = _mapper.Map<IEnumerable<CancelReasonDto>>(cancelReasons);
            return new PagedResponse<CancelReasonDto>
            {
                Items = cancleReasonDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<CancelReasonDto> CreateAsync(CancelReasonForCreationDto cancelReasonDto)
        {
            if (cancelReasonDto == null)
                throw new ArgumentNullException(nameof(cancelReasonDto), "Cancel reason data cannot be null.");
            var cancelReason = _mapper.Map<CancelReason>(cancelReasonDto);
            _unitOfWork.CancelReasons.Add(cancelReason);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CancelReasonDto>(cancelReason);
        }

        public async Task<CancelReasonDto> UpdateAsync(CancelReasonForUpdateDto cancelReasonDto)
        {
            if (cancelReasonDto == null)
                throw new ArgumentNullException(nameof(cancelReasonDto), "Cancel reason data cannot be null.");
            var cancelReason = await _unitOfWork.CancelReasons.GetByIdAsync(cancelReasonDto.Id);
            if (cancelReason == null)
                throw new KeyNotFoundException($"Cancel reason with ID {cancelReasonDto.Id} not found.");
            _mapper.Map(cancelReasonDto, cancelReason);
            _unitOfWork.CancelReasons.Update(cancelReason);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CancelReasonDto>(cancelReason);
        }

        public async Task DeleteAsync(Guid id)
        {
            var cancelReason = await _unitOfWork.CancelReasons.GetByIdAsync(id);
            if (cancelReason == null)
                throw new KeyNotFoundException($"Cancel reason with ID {id} not found.");
            _unitOfWork.CancelReasons.Delete(cancelReason);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
