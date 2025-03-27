using AutoMapper;
using BusinessObjects.Dto.Voucher;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using Services.Interface;
using Services.Response;
using Tools;

namespace Services.Implementation;

public class VoucherService : IVoucherService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VoucherService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<VoucherDto> GetByIdAsync(Guid id)
    {
        var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
        if (voucher == null || voucher.IsDeleted)
            throw new KeyNotFoundException($"Voucher with ID {id} not found.");
        return _mapper.Map<VoucherDto>(voucher);
    }

    public async Task<PagedResponse<VoucherDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (vouchers, totalCount) = await _unitOfWork.Vouchers.GetPagedAsync(pageNumber, pageSize, v => v.IsDeleted == false);
        var voucherDtos = _mapper.Map<IEnumerable<VoucherDto>>(vouchers);
        return new PagedResponse<VoucherDto>
        {
            Items = voucherDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<VoucherDto> CreateAsync(VoucherForCreationDto? voucherForCreationDto)
    {
        if (voucherForCreationDto is null)
            throw new ArgumentNullException(nameof(voucherForCreationDto), "Voucher data cannot be null.");

        // Check if a voucher with the same name already exists
        var existingVoucher = await _unitOfWork.Vouchers.Entities
            .FirstOrDefaultAsync(v => v.Code == voucherForCreationDto.Code && !v.IsDeleted);
        if (existingVoucher != null)
            throw new ArgumentException($"Mã giảm giá '{voucherForCreationDto.Code}' đã tồn tại");

        var voucher = _mapper.Map<Voucher>(voucherForCreationDto);
        voucher.Id = Guid.NewGuid();
        voucher.Code = voucherForCreationDto.Code.ToUpper();
        voucher.CreatedTime = DateTimeOffset.UtcNow;
        voucher.CreatedBy = "System";
        voucher.IsDeleted = false;
        _unitOfWork.Vouchers.Add(voucher);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<VoucherDto>(voucher);
    }

    public async Task<VoucherDto> UpdateAsync(Guid voucherId, VoucherForUpdateDto voucherForUpdateDto)
    {
        if (voucherForUpdateDto is null)
            throw new ArgumentNullException(nameof(voucherForUpdateDto), "Voucher data cannot be null.");

        var voucher = await _unitOfWork.Vouchers.GetByIdAsync(voucherId);
        if (voucher == null)
            throw new KeyNotFoundException($"Voucher with ID {voucherId} not found.");

        // Check if the voucher code already exists for another voucher
        if (await _unitOfWork.Vouchers.GetQueryable().AnyAsync(v => v.Code == voucherForUpdateDto.Code && v.Id != voucherId))
            throw new InvalidOperationException($"Voucher code {voucherForUpdateDto.Code} is already in use.");

        voucher.LastUpdatedTime = DateTimeOffset.UtcNow;
        voucher.LastUpdatedBy = "System";

        _mapper.Map(voucherForUpdateDto, voucher);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<VoucherDto>(voucher);
    }

    public async Task DeleteAsync(Guid id)
    {
        var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
        if (voucher == null)
            throw new KeyNotFoundException($"Voucher with ID {id} not found.");
        voucher.IsDeleted = true;
        voucher.DeletedTime = DateTimeOffset.UtcNow;
        voucher.DeletedBy = "System";
        _unitOfWork.Vouchers.Update(voucher);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<VoucherDto> GetByCodeAsync(string voucherCode)
    {
        if (string.IsNullOrWhiteSpace(voucherCode))
            throw new ArgumentException("Voucher code cannot be null or empty.", nameof(voucherCode));

        // Tìm voucher bằng voucherCode
        var voucher = await _unitOfWork.Vouchers.Entities
            .FirstOrDefaultAsync(v => v.Code == voucherCode && !v.IsDeleted);

        if (voucher == null)
            throw new KeyNotFoundException($"Voucher with code {voucherCode} not found.");

        return _mapper.Map<VoucherDto>(voucher);
    }
}
