using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestMaker.Database.Entities;
using TestMaker.Database.Models;

namespace TestMaker.Database.Services
{
    public interface IDatabaseService
    {
        public Task AddUserAsync(string username, string password);

        public Task AddUserAsync(User user);

        public Task<List<User>> GetUsersAsync();

        public Task<User> GetUserAsync(int id);

        public Task<User> GetUserAsync(string username, string password);

        public Task<bool> IsUserExistsAsync(string username);

        public Task UpdateUserAsync(User user);
    }
}