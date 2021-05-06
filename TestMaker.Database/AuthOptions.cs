using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace TestMaker.Database
{
    public static class AuthOptions
    {
        public const string ISSUER = "TestMakerServer"; // издатель токена
        public const string AUDIENCE = "TestMakerClient"; // потребитель токена
        private const string KEY = "randomKey123randomKey321";   // ключ для шифрации
        public const int LIFETIME = 5; // время жизни токена

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}