using System;
using System.Collections.Generic;
using System.Text;

namespace TestMaker.Database.Models
{
    public class UsersDTO
    {
        public int Id { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public UsersDTO()
        {
        }
    }
}