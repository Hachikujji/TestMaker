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
        #region Private Fields

        private const string ISSUER = "TestMakerServer";
        private const string AUDIENCE = "TestMakerClient";
        private const string KEY = "randomKey123randomKey321";
        private const int LIFETIME = 5; // mins
        private const int REFRESH_LIFETIME = 4; // hours

        private JwtSecurityTokenHandler _TokenHandler = new JwtSecurityTokenHandler();

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Get token validation parameters
        /// </summary>
        /// <returns> Token validation parameters</returns>
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

        /// <summary>
        /// Get symmetric security key
        /// </summary>
        /// <returns>Symmetric Security Key</returns>
        public SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }

        /// <summary>
        /// Creates jwt token
        /// </summary>
        /// <returns>string - JWT token</returns>
        public string CreateJwtToken()
        {
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                    issuer: ISSUER,
                    audience: AUDIENCE,
                    notBefore: now,
                    expires: now.Add(TimeSpan.FromMinutes(LIFETIME)),
                    signingCredentials: new SigningCredentials(GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        /// <summary>
        ///  Creates refresh token
        /// </summary>
        /// <returns> Refresh token </returns>
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

        /// <summary>
        /// Validates token
        /// </summary>
        /// <param name="JwtToken"> JWT token </param>
        /// <returns> true if token is valid, else returns false</returns>
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

        #endregion Public Methods
    }
}