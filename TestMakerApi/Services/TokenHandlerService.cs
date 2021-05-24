using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TestMaker.Database.Entities;

namespace TestMakerApi.Services
{
    public class TokenHandlerService : ITokenHandlerService
    {
        private const string ISSUER = "TestMakerServer";
        private const string AUDIENCE = "TestMakerClient";
        private const string KEY = "randomKey123randomKey321";
        private const int LIFETIME = 10; // mins
        private const int REFRESH_LIFETIME = 4; // hours

        private JwtSecurityTokenHandler _TokenHandler = new JwtSecurityTokenHandler();

        public TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = true,
                ValidIssuer = ISSUER,
                ClockSkew = TimeSpan.Zero,
                ValidAudience = AUDIENCE,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY))
            };
        }

        public SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }

        public string CreateJwtToken()
        {
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                    issuer: ISSUER,
                    audience: AUDIENCE,
                    notBefore: now,
                    expires: now.Add(TimeSpan.FromSeconds(LIFETIME)),
                    signingCredentials: new SigningCredentials(GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        public RefreshToken CreateRefreshToken()
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddHours(REFRESH_LIFETIME),
                    Created = DateTime.UtcNow
                };
            }
        }

        public bool ValidateToken(string JwtToken)
        {
            try
            {
                SecurityToken validated = null;
                _TokenHandler.ValidateToken(JwtToken, GetTokenValidationParameters(), out validated);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}