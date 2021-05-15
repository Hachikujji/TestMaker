using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using TestMaker.Database.Entities;

namespace TestMaker.Database.Models
{
    public class UserAuthenticationResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }

        public UserAuthenticationResponse(User user, string jwtToken, string refreshToken)
        {
            Id = user.Id;
            Username = user.Username;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
        }
        public UserAuthenticationResponse()
        {

        }
    }
}