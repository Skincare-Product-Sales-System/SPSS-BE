using BusinessObjects;
using BusinessObjects.Models;
using Repositories.Implementation;
using Repositories.Interface;
using System;

namespace Repositories.Implementation
{
    public class SkinAnalysisResultRepository : RepositoryBase<SkinAnalysisResult, Guid>, ISkinAnalysisResultRepository
    {
        public SkinAnalysisResultRepository(SPSSContext context) : base(context)
        {
        }
    }
}