using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestMaker.Database.Models
{
    public class UserAuthenticationRequest
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public UserAuthenticationRequest(string u, string p)
        {
            Username = u;
            Password = p;
        }

        public UserAuthenticationRequest()
        {
        }
    }
}