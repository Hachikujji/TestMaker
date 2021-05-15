using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestMaker.Database.Entities;

namespace TestMakerApi.Services
{
    public interface ITokenHandlerService
    {
        public TokenValidationParameters GetTokenValidationParameters();

        public SymmetricSecurityKey GetSymmetricSecurityKey();

        public string CreateJwtToken();

        public RefreshToken CreateRefreshToken();

        public bool ValidateToken(string JwtToken);
    }
}