using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class PaymentMethodRepository : RepositoryBase<PaymentMethod, Guid>, IPaymentMethodRepository
    {
        public PaymentMethodRepository(SPSSContext repositoryContext)
                : base(repositoryContext)
        {
        }
    }
}
