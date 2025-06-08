using BusinessObjects;
using BusinessObjects.Models;
using Repositories.Implementation;
using Repositories.Interface;
using System;

namespace Repositories.Implementation
{
    public class SkinAnalysisRecommendationRepository : RepositoryBase<SkinAnalysisRecommendation, Guid>, ISkinAnalysisRecommendationRepository
    {
        public SkinAnalysisRecommendationRepository(SPSSContext context) : base(context)
        {
        }
    }
}