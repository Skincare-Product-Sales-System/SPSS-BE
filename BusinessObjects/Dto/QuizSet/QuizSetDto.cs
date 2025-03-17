using BusinessObjects.Models;

namespace BusinessObjects.Dto.QuizSet;

public class QuizSetDto : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsDefault { get; set; }
}