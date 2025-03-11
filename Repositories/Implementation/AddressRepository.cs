using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation;

public class AddressRepository : RepositoryBase<Address, Guid>, IAddressRepository
{
    public AddressRepository(SPSSContext context) : base(context)
    {
    }
}
