using BusinessObjects.Dto.QuizQuestion;
using BusinessObjects.Dto.QuizResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dto.QuizSet
{
    public class QuizSetQuestionAndAnswerDto
    {
        public Guid Id { get; set; }
        public string QuizSetName { get; set; }
        public List<QuizQuestionAndAnswerDto> QuizQuestions { get; set; }
    }
}
