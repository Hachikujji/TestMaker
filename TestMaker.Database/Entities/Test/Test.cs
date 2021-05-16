using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace TestMaker.Database.Entities
{
    public class Test
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public User Creator { get; set; }
        public int Attempts { get; set; }
        public IList<TestQuestion> Questions { get; set; }

        public Test(string name, User creator, int attempts, IList<TestQuestion> questions)
        {
            Name = name;
            Creator = creator;
            Attempts = attempts;
            Questions = questions;
        }

        public Test()
        {
            Questions = new List<TestQuestion>();
        }
    }
}