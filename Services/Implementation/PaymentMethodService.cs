using AutoMapper;
using BusinessObjects.Dto.PaymentMethod;
using BusinessObjects.Models;
using Repositories.Interface;
using Services.Interface;
using Services.Response;

namespace Services.Implementation
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly string _currentUser; // Placeholder for current user, ideally injected

        public PaymentMethodService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _currentUser = "System"; // Replace with actual user retrieval (e.g., from IHttpContextAccessor or a service)
        }

        public async Task<PaymentMethodDto> GetByIdAsync(Guid id)
        {
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(id);
            if (paymentMethod == null || paymentMethod.IsDeleted)
                throw new KeyNotFoundException($"Payment method with ID {id} not found or has been deleted.");

            return _mapper.Map<PaymentMethodDto>(paymentMethod);
        }

        public async Task<PagedResponse<PaymentMethodDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var (paymentMethods, totalCount) = await _unitOfWork.PaymentMethods.GetPagedAsync(
                pageNumber,
                pageSize,
                pm => pm.IsDeleted == false // Filter out deleted payment methods
            );
            var paymentMethodDtos = _mapper.Map<IEnumerable<PaymentMethodDto>>(paymentMethods);
            return new PagedResponse<PaymentMethodDto>
            {
                Items = paymentMethodDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PaymentMethodDto> CreateAsync(PaymentMethodForCreationDto paymentMethodDto, string userId)
        {
            if (paymentMethodDto == null)
                throw new ArgumentNullException(nameof(paymentMethodDto), "Payment method data cannot be null.");

            var paymentMethod = _mapper.Map<PaymentMethod>(paymentMethodDto);
            paymentMethod.Id = Guid.NewGuid();
            paymentMethod.CreatedTime = DateTimeOffset.UtcNow;
            paymentMethod.CreatedBy = userId;
            paymentMethod.LastUpdatedTime = DateTimeOffset.UtcNow;
            paymentMethod.LastUpdatedBy = userId;
            paymentMethod.IsDeleted = false;

            _unitOfWork.PaymentMethods.Add(paymentMethod);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PaymentMethodDto>(paymentMethod);
        }

        public async Task<PaymentMethodDto> UpdateAsync(Guid id, PaymentMethodForUpdateDto paymentMethodDto, string userId)
        {
            if (paymentMethodDto == null)
                throw new ArgumentNullException(nameof(paymentMethodDto), "Payment method data cannot be null.");

            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(id);
            if (paymentMethod == null || paymentMethod.IsDeleted)
                throw new KeyNotFoundException($"Payment method with ID {id} not found or has been deleted.");

            paymentMethod.LastUpdatedTime = DateTimeOffset.UtcNow;
            paymentMethod.LastUpdatedBy = userId;

            _mapper.Map(paymentMethodDto, paymentMethod);
            _unitOfWork.PaymentMethods.Update(paymentMethod);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<PaymentMethodDto>(paymentMethod);
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(id);
            if (paymentMethod == null || paymentMethod.IsDeleted)
                throw new KeyNotFoundException($"Payment method with ID {id} not found or has been deleted.");

            paymentMethod.IsDeleted = true;
            paymentMethod.DeletedTime = DateTimeOffset.UtcNow;
            paymentMethod.DeletedBy = userId;

            _unitOfWork.PaymentMethods.Update(paymentMethod); // Soft delete via update
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
