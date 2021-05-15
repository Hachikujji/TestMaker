using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestMaker.Database.Entities
{
    public class TestQuestion
    {
        [Key]
        public int Id { get; set; }

        public string Question { get; set; }
        public List<TestAnswer> Answers { get; set; }

        public TestQuestion(string question, List<TestAnswer> answers)
        {
            Question = question;
            Answers = answers;
        }

        public TestQuestion()
        {
        }
    }
}