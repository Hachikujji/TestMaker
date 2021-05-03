using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestMaker.Database.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public Users(string u, string p)
        {
            Username = u;
            Password = p;
        }

        public Users()
        {
        }
    }
}