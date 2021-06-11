using System;
using System.Collections.Generic;
using System.Text;
using TestMaker.Database.Entities;
using TestMaker.Database.Models;

namespace TestMaker.Stuff
{
    // not good idea to use same class for Regions, HttpClient, And local storage for user
    public static class StaticProperties
    {
        // Use Summary doc
        //S hell region
        public const string ContentRegion = "MainRegion";

        // Http client
        public static System.Net.Http.HttpClient Client { get; set; }

        // User responce header: id, username, jwt token, refresh token
        public static UserAuthenticationResponse CurrentUserResponseHeader { get; set; }
    }
}