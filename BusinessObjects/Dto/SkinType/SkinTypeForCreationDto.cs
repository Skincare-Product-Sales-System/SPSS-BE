using BusinessObjects.Dto.SkincareRoutinStep;

namespace BusinessObjects.Dto.SkinType;

public class SkinTypeForCreationDto
{
    
    public string Name { get; set; }

    public string Description { get; set; }
    public List<SkinTypeRoutineStepForCreationDto> SkinTypeRoutineSteps { get; set; } = new List<SkinTypeRoutineStepForCreationDto>();
}