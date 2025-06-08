using BusinessObjects.Dto.SkinAnalysis;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface ISkinAnalysisService
    {
        Task<SkinAnalysisResultDto> AnalyzeSkinAsync(IFormFile faceImage);
    }
}