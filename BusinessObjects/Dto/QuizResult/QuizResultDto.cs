using BusinessObjects.Dto.SkinType;

namespace BusinessObjects.Dto.QuizResult;

public class QuizResultDto
{
    public Guid Id { get; set; }

    public string Score { get; set; }

    public Guid SkinTypeId { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public string Routine { get; set; }
}