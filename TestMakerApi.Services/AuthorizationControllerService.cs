using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestMaker.Database.Entities;
using TestMaker.Database.Services;

namespace TestMakerApi.Services
{
    public class AuthorizationControllerService : IAuthorizationControllerService
    {
        #region Public Constructors

        public AuthorizationControllerService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        #endregion Public Constructors

        #region Public Methods

        public async Task AddUserAsync(User user)
        {
            await _databaseService.AddUserAsync(user);
        }

        public async Task<User> GetUserAsync(string username, string password)
        {
            var user = await _databaseService.GetUserAsync(username, password);
            return user;
        }

        public async Task<User> GetUserByIdAsync(int UserId)
        {
            var user = await _databaseService.GetUserAsync(UserId);
            return user;
        }

        public async Task<IList<string>> GetUsernamesAsync()
        {
            var list = await _databaseService.GetUsernamesAsync();
            return list;
        }

        public async Task<bool> IsUserExistsAsync(string username)
        {
            var isExists = await _databaseService.IsUserExistsAsync(username);
            return isExists;
        }

        public async Task UpdateUserRefreshTokenAsync(User user, RefreshToken refreshToken)
        {
            await _databaseService.UpdateUserRefreshTokenAsync(user, refreshToken);
        }

        #endregion Public Methods

        #region Private Fields

        private readonly IDatabaseService _databaseService;

        #endregion Private Fields
    }
}