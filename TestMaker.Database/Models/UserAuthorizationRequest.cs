using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestMaker.Database.Models
{
    public class UserAuthorizationRequest
    {
        public string Username { get;set; }

        public string Password { get; set; }
    }
}