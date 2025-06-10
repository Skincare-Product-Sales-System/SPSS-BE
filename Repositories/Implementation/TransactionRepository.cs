using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Implementation;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class TransactionRepository : RepositoryBase<Transaction, Guid>, ITransactionRepository
    {
        public TransactionRepository(SPSSContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Transaction>> GetPendingTransactionsAsync()
        {
            return await Entities
                .Where(t => t.Status == "Pending" && !t.IsDeleted)
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(Guid userId)
        {
            return await Entities
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .OrderByDescending(t => t.CreatedTime)
                .ToListAsync();
        }

        public async Task<Transaction> GetTransactionByIdAsync(Guid id)
        {
            return await Entities
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByStatusAsync(string status)
        {
            return await Entities
                .Where(t => t.Status == status && !t.IsDeleted)
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedTime)
                .ToListAsync();
        }
    }
}