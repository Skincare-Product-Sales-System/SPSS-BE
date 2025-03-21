using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.QuizResult;

public class QuizResultForUpdateDto
{
    [Required(ErrorMessage = "Score is required.")]
    [StringLength(50, ErrorMessage = "Score can't exceed 50 characters.")]
    public string Score { get; set; }
}