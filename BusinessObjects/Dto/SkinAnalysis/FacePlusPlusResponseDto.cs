using System;
using System.Collections.Generic;

namespace BusinessObjects.Dto.SkinAnalysis
{
    /// <summary>
    /// Represents the raw Face++ API response structure
    /// </summary>
    public class FacePlusPlusResponseDto
    {
        public string RequestId { get; set; }
        public int TimeUsed { get; set; }
        public List<FaceDto> Faces { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class FaceDto
    {
        public string FaceToken { get; set; }
        public FaceRectangleDto FaceRectangle { get; set; }
        public FaceAttributesDto Attributes { get; set; }
    }

    public class FaceRectangleDto
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class FaceAttributesDto
    {
        public GenderDto Gender { get; set; }
        public AgeDto Age { get; set; }
        public SkinStatusDto SkinStatus { get; set; }
    }

    public class GenderDto
    {
        public string Value { get; set; }
    }

    public class AgeDto
    {
        public int Value { get; set; }
    }

    public class SkinStatusDto
    {
        public float Health { get; set; }
        public float Stain { get; set; }
        public float Acne { get; set; }
        public float DarkCircle { get; set; }
    }
}