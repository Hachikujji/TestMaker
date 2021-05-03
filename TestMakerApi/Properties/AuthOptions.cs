using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace TestMakerApi.Properties
{
    public static class AuthOptions
    {
        public const string ISSUER = "TestMakerServer"; // издатель токена
        public const string AUDIENCE = "TestMakerClient"; // потребитель токена
        private const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
        public const int LIFETIME = 5; // время жизни токена - 1 минута

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}