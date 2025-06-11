using BusinessObjects.Dto.Transaction;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface ITransactionService
    {
        Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto dto, Guid userId);
        Task<TransactionDto> GetTransactionByIdAsync(Guid id);
        Task<IEnumerable<TransactionDto>> GetTransactionsByUserIdAsync(Guid userId);
        Task<PagedResponse<TransactionDto>> GetPagedTransactionsAsync(int pageNumber, int pageSize, string status = null);
        Task<TransactionDto> UpdateTransactionStatusAsync(UpdateTransactionStatusDto dto, string adminId);
        Task<string> GenerateQrCodeAsync(decimal amount, string description);
    }
}