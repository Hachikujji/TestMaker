using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestMaker.Database.Entities
{
    public class TestResult
    {
        [Key]
        public int Id { get; set; }

        public User User { get; set; }
        public Test Test { get; set; }
        public DateTime Date { get; set; }
        public float Result { get; set; }
        public IList<TestResultQuestion> Questions { get; set; }

        public TestResult(Test test, User user, DateTime date, float result, IList<TestResultQuestion> questions)
        {
            Test = test;
            User = user;
            Date = date;
            Result = result;
            Questions = questions;
        }

        public TestResult(Test test)
        {
            Test = test;
            Result = 0;
            Questions = new ObservableCollection<TestResultQuestion>();
            foreach (var question in test.Questions)
            {
                Questions.Add(new TestResultQuestion(question));
            }
        }

        public TestResult()
        {
        }
    }
}