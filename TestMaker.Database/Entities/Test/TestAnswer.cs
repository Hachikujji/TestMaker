using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestMaker.Database.Entities
{
    public class TestAnswer
    {
        [Key]
        public int Id { get; set; }

        public string Answer { get; set; }
        public bool IsRight { get; set; }

        public TestAnswer(string answer, bool isRight)
        {
            Answer = answer;
            IsRight = isRight;
        }

        public TestAnswer()
        {
        }
    }
}