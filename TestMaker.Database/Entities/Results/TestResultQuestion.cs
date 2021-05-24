using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestMaker.Database.Entities
{
    public class TestResultQuestion
    {
        [Key]
        public int Id { get; set; }

        public string Question { get; set; }
        public IList<TestResultAnswer> Answers { get; set; }

        public TestResultQuestion(string question, IList<TestResultAnswer> answers)
        {
            Question = question;
            Answers = answers;
        }

        public TestResultQuestion(TestQuestion question)
        {
            Question = question.Question;
            Answers = new ObservableCollection<TestResultAnswer>();
            foreach (var answer in question.Answers)
            {
                Answers.Add(new TestResultAnswer(answer));
            }
        }

        public TestResultQuestion()
        {
            Answers = new ObservableCollection<TestResultAnswer>();
        }
    }
}