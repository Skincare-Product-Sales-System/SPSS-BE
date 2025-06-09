using System;
using System.Collections.Generic;

namespace BusinessObjects.Dto.SkinAnalysis
{
    public class SkinAnalysisResultDto
    {
        public string ImageUrl { get; set; }
        public SkinConditionDto SkinCondition { get; set; }
        public List<SkinIssueDto> SkinIssues { get; set; }
        public List<ProductRecommendationDto> RecommendedProducts { get; set; }
        public List<SkincareRoutineStepDto> RoutineSteps { get; set; } = new List<SkincareRoutineStepDto>();
        public List<string> SkinCareAdvice { get; set; }
    }

    public class SkinConditionDto
    {
        public int AcneScore { get; set; }
        public int WrinkleScore { get; set; }
        public int DarkCircleScore { get; set; }
        public int DarkSpotScore { get; set; }
        public int HealthScore { get; set; }
        public string SkinType { get; set; }
    }

    public class SkinIssueDto
    {
        public string IssueName { get; set; }
        public string Description { get; set; }
        public int Severity { get; set; } // 1-10 scale
    }

    public class ProductRecommendationDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string RecommendationReason { get; set; }
        public int PriorityScore { get; set; } // 1-10 scale, higher means more recommended
    }
}