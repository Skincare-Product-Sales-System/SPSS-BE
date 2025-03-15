using BusinessObjects.Models;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class SkinTypeRoutineStepRepository : RepositoryBase<SkinTypeRoutineStep, Guid>, ISkinTypeRoutineStepRepository
    {
        public SkinTypeRoutineStepRepository(SPSSContext dbContext) : base(dbContext)
        {
        }
    }
}
