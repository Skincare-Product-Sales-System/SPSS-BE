using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.QuizOption;

public class QuizOptionForCreationDto
{
    [Required(ErrorMessage = "Option value is required.")]
    [StringLength(200, ErrorMessage = "Option value can't exceed 200 characters.")]
    public string Value { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Score must be a non-negative integer.")]
    public int Score { get; set; }
}