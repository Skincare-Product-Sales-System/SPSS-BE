using BusinessObjects.Dto.SkinAnalysis;
using Microsoft.AspNetCore.Http;
using Services.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface ISkinAnalysisService
    {
        Task<SkinAnalysisResultDto> AnalyzeSkinAsync(IFormFile faceImage, Guid userId);
        Task<SkinAnalysisResultDto> GetSkinAnalysisResultByIdAsync(Guid id);
        Task<List<SkinAnalysisResultDto>> GetSkinAnalysisResultsByUserIdAsync(Guid userId);
        Task<PagedResponse<SkinAnalysisResultDto>> GetPagedSkinAnalysisResultsByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
    }
}