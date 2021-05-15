using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestMaker.Database.Entities
{
    public class TestAccess
    {
        [Key]
        public int Id { get; set; }

        public User User { get; set; }
        public Test Test { get; set; }
        public int Role { get; set; }

        public TestAccess(User user, Test test, int role)
        {
            User = user;
            Test = test;
            Role = role;
        }

        public TestAccess()
        {
        }
    }
}