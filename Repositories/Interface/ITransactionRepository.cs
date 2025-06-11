using BusinessObjects.Models;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface ITransactionRepository : IRepositoryBase<Transaction, Guid>
    {
        Task<IEnumerable<Transaction>> GetPendingTransactionsAsync();
        Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(Guid userId);
        Task<Transaction> GetTransactionByIdAsync(Guid id);
        Task<IEnumerable<Transaction>> GetTransactionsByStatusAsync(string status);
    }
}