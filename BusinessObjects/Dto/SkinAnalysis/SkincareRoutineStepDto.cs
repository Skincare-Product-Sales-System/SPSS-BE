using System;
using System.Collections.Generic;

namespace BusinessObjects.Dto.SkinAnalysis
{
    public class SkincareRoutineStepDto
    {
        public string StepName { get; set; }
        public string Instruction { get; set; }
        public int Order { get; set; }
        public List<ProductRecommendationDto> Products { get; set; } = new List<ProductRecommendationDto>();
    }
}