using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestMaker.Database.Entities;

namespace TestMakerApi.Services
{
    public interface IAuthorizationControllerService
    {
        public Task AddUserAsync(User user);

        public Task<User> GetUserAsync(string username, string password);

        public Task<User> GetUserByIdAsync(int UserId);

        public Task<IList<string>> GetUsernamesAsync();

        public Task<bool> IsUserExistsAsync(string username);

        public Task UpdateUserRefreshTokenAsync(User user, RefreshToken refreshToken);
    }
}