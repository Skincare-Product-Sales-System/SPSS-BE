using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.QuizQuestion;

public class QuizQuestionForUpdateDto
{
    [Required(ErrorMessage = "Question value is required.")]
    [StringLength(500, ErrorMessage = "Question value can't exceed 500 characters.")]
    public string Value { get; set; }
}