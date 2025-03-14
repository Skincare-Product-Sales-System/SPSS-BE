using BusinessObjects.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface ICountryRepository
    {
        Task<Country?> GetByIdAsync(int id);
        Task<IEnumerable<Country>> GetAllAsync();
        Task AddAsync(Country country);
        Task UpdateAsync(Country country);
        Task DeleteAsync(int id);
    }
}
