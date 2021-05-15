using System;
using System.Collections.Generic;
using System.Text;
using TestMaker.Database.Entities;

namespace TestMaker.Database.Models
{
    public class UserAuthorizationRequest
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }

        public UserAuthorizationRequest(User user, string jwtToken)
        {
            Id = user.Id;
            Username = user.Username;
            JwtToken = jwtToken;
        }

        public UserAuthorizationRequest(UserAuthenticationResponse response)
        {
            Id = response.Id;
            Username = response.Username;
            JwtToken = response.JwtToken;
        }

        public UserAuthorizationRequest()
        {
        }
    }
}