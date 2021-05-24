using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace TestMaker.Database.Entities
{
    public class Test
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string CreatorName { get; set; }
        public int Attempts { get; set; }
        public IList<TestQuestion> Questions { get; set; }

        public Test(string name, string creatorName, int attempts, IList<TestQuestion> questions)
        {
            Name = name;
            CreatorName = creatorName;
            Attempts = attempts;
            Questions = questions;
        }

        public Test(Test test)
        {
            Id = test.Id;
            Name = test.Name;
            CreatorName = test.CreatorName;
            Attempts = test.Attempts;
            Questions = new ObservableCollection<TestQuestion>(test.Questions);
            for (int i = 0; i < test.Questions.Count; i++)
                Questions[i].Answers = new ObservableCollection<TestAnswer>(test.Questions[i].Answers);
        }

        public Test()
        {
            Questions = new List<TestQuestion>();
        }
    }
}