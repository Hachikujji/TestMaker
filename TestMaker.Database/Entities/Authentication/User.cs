using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using TestMaker.Database.Entities;

namespace TestMaker.Database.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Username { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        [JsonIgnore]
        public RefreshToken RefreshToken { get; set; }

        public User()
        {
        }

        public User(string u, string p)
        {
            Username = u;
            Password = p;
        }
    }
}