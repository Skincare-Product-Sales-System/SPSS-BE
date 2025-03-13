using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation;

public class SkinTypeRepository : RepositoryBase<SkinType, Guid>, ISkinTypeRepository
{
    public SkinTypeRepository(SPSSContext context) : base(context)
    {
    }
}
