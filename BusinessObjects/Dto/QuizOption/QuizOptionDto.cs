namespace BusinessObjects.Dto.QuizOption;

public class QuizOptionDto
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public string Value { get; set; }

    public int Score { get; set; }

}