using System;
using System.Collections.Generic;
using System.Text;
using TestMaker.Database.Entities;

namespace TestMaker.Database.Models
{
    public class AddTestAccessRequest
    {
        public string Username { get; set; }
        public Test Test { get; set; }

        public AddTestAccessRequest()
        {
        }

        public AddTestAccessRequest(string username, Test test)
        {
            Username = username;
            Test = test;
        }
    }
}