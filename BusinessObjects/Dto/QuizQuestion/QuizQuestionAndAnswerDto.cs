using BusinessObjects.Dto.QuizOption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dto.QuizQuestion
{
    public class QuizQuestionAndAnswerDto
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
        public List<QuizOptionDto> QuizOptions { get; set; }
    }
}
