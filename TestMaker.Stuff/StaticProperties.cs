using System;
using System.Collections.Generic;
using System.Text;
using TestMaker.Database.Entities;
using TestMaker.Database.Models;

namespace TestMaker.Stuff
{
    public static class StaticProperties
    {
        //S hell region
        public const string ContentRegion = "MainRegion";

        // Http client
        public static System.Net.Http.HttpClient Client { get; set; }

        // User responce header: id, username, jwt token, refresh token
        public static UserAuthenticationResponse CurrentUserResponseHeader { get; set; }
    }
}