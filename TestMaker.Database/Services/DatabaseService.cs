using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TestMaker.Database.Entities;
using TestMaker.Database.Models;

namespace TestMaker.Database.Services
{
    public class DatabaseService : IDatabaseService
    {
        public async Task AddUserAsync(string username, string password)
        {
            using (var db = new DatabaseContext())
            {
                User user = new User(username, password);
                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();
            }
        }

        public async Task AddUserAsync(User user)
        {
            using (var db = new DatabaseContext())
            {
                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();
            }
        }

        public async Task<bool> IsUserExistsAsync(string username)
        {
            using (var db = new DatabaseContext())
            {
                var u = await db.Users.FirstOrDefaultAsync(x => x.Username == username);
                if (u == null)
                    return false;
                return true;
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Users.ToListAsync();
            }
        }

        public async Task<User> GetUserAsync(int id)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Users.FindAsync(id);
            }
        }

        public async Task<User> GetUserAsync(string username, string password)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Users.FirstOrDefaultAsync(x => x.Username == username && x.Password == password);
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            using (var db = new DatabaseContext())
            {
                db.Update(user);
                await db.SaveChangesAsync();
            }
        }

        public string CreateJwtToken(User user)
        {
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Id.ToString())
                    },
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
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
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow
                };
            }
        }
    }
}