using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestMaker.Database.Entities
{
    public class TestResultAnswer
    {
        [Key]
        public int Id { get; set; }

        public string Answer { get; set; }
        public bool IsRight { get; set; }
        public bool IsSelected { get; set; }

        public TestResultAnswer(string answer, bool isRight, bool isSelected)
        {
            Answer = answer;
            IsRight = isRight;
            IsSelected = isSelected;
        }

        public TestResultAnswer()
        {
        }

        public TestResultAnswer(TestAnswer answer)
        {
            Answer = answer.Answer;
            IsRight = answer.IsRight;
            IsSelected = false;
        }
    }
}