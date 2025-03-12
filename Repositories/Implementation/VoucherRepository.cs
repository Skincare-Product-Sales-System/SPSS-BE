using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation;

public class VoucherRepository : RepositoryBase<Voucher, Guid>, IVoucherRepository
{
    public VoucherRepository(SPSSContext context) : base(context)
    {
    }
}