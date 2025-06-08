using BusinessObjects;
using BusinessObjects.Models;
using Repositories.Implementation;
using Repositories.Interface;
using System;

namespace Repositories.Implementation
{
    public class SkinAnalysisIssueRepository : RepositoryBase<SkinAnalysisIssue, Guid>, ISkinAnalysisIssueRepository
    {
        public SkinAnalysisIssueRepository(SPSSContext context) : base(context)
        {
        }
    }
}