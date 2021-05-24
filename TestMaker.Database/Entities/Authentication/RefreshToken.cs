using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace TestMaker.Database.Entities
{
    [Owned]
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public RefreshToken(string token, DateTime expires, DateTime created)
        {
            Token = token;
            Expires = expires;
            Created = created;
        }

        public RefreshToken()
        {
        }
    }
}