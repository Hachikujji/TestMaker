using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace TestMaker.Database.Entities
{
    public class Test : IValidatableObject
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

        public Test()
        {
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }
}