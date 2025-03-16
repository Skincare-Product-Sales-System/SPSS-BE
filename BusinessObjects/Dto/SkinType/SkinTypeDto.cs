using BusinessObjects.Dto.SkincareRoutinStep;

namespace BusinessObjects.Dto.SkinType;

public class SkinTypeDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }
    public List<SkinTypeRoutineStepDto> SkinTypeRoutineSteps { get; set; } = new List<SkinTypeRoutineStepDto>();
}