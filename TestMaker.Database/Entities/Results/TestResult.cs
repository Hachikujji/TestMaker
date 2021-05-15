using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestMaker.Database.Entities
{
    public class TestResult
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public User User { get; set; }
        public DateTime Date { get; set; }
        public float Result { get; set; }
        public List<TestResultAnswer> Answers { get; set; }

        public TestResult(string name, User user, DateTime date, float result, List<TestResultAnswer> answers)
        {
            Name = name;
            User = user;
            Date = date;
            Result = result;
            Answers = answers;
        }

        public TestResult()
        {
        }
    }
}