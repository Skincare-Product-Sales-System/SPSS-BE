using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class OrderRepository : RepositoryBase<Order, Guid>, IOrderRepository
    {
        public OrderRepository(SPSSContext repositoryContext) : base(repositoryContext)
        {
        }
    }
}
