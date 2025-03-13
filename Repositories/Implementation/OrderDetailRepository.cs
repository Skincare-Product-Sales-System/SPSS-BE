using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class OrderDetailRepository : RepositoryBase<OrderDetail, Guid>, IOrderDetailRepository
    {
        public OrderDetailRepository(SPSSContext context) : base(context)
        {
        }
    }
}
