using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestMaker.Database.Models;

namespace TestMaker.Database.Services
{
    public interface IDatabaseService
    {
        public void AddUser(string username, string password);

        public void AddUser(Users user);

        public Task<List<Users>> GetUsers();

        public Task<Users> GetUser(int id);

        public Task<Users> GetUser(string username, string password);
    }
}