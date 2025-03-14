using BusinessObjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface ICountryRepository : IRepositoryBase<Country, Guid>
    {
    }
}
