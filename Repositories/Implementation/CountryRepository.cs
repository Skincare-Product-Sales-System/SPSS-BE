using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation;

public class CountryRepository : RepositoryBase<Country, Guid>, ICountryRepository
{
    public CountryRepository(SPSSContext context) : base(context)
    {
    }
}