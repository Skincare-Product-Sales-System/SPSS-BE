using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.QuizSet;

public class QuizSetForUpdateDto
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, ErrorMessage = "Name can't exceed 200 characters.")]
    public string Name { get; set; }

    public bool IsDefault { get; set; }
}