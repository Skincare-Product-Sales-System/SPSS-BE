using System.Collections.Generic;

namespace BusinessObjects.Dto.SkinAnalysis
{
    public class EnhancedSkinAnalysisDto
    {
        public int AcneScore { get; set; }
        public int WrinkleScore { get; set; }
        public int DarkCircleScore { get; set; }
        public int DarkSpotScore { get; set; }
        
        // Các thông tin nâng cao từ TensorFlow/EfficientNet
        public int PoreSize { get; set; }
        public int DrynessLevel { get; set; }
        public int OilinessLevel { get; set; }
        public int SensitivityLevel { get; set; }
        public int RednessLevel { get; set; }
        
        public string EnhancedSkinType { get; set; }
        public List<EnhancedSkinIssueDto> EnhancedSkinIssues { get; set; }
    }
    
    public class EnhancedSkinIssueDto
    {
        public string IssueName { get; set; }
        public string Description { get; set; }
        public int Severity { get; set; }
        public List<string> RecommendedIngredients { get; set; }
        public List<string> AvoidIngredients { get; set; }
    }
}