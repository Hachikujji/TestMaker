using System;
using System.Collections.Generic;
using System.Text;
using TestMaker.Database.Entities;
using TestMaker.Database.Models;

namespace TestMaker.Stuff
{
    public static class StaticProperties
    {
        public const string ContentRegion = "MainRegion";
        public static System.Net.Http.HttpClient Client { get; set; }
        public static UserAuthenticationResponse CurrentUserResponseHeader { get; set; }
    }
}