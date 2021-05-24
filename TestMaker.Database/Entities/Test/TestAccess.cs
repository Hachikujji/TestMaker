using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TestMaker.Database.Entities
{
    public class TestAccess
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public User User { get; set; }

        public Test Test { get; set; }

        public TestAccess(User user, Test test)
        {
            Id = 0;
            User = user;
            Test = test;
        }

        public TestAccess()
        {
        }
    }
}