using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class CountryRepository : RepositoryBase<Country, Guid> , ICountryRepository
    {
        public CountryRepository(SPSSContext context) : base(context)
        {
        }
    }
}
