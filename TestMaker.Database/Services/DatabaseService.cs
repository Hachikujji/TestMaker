using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestMaker.Database.Models;

namespace TestMaker.Database.Services
{
    public class DatabaseService : IDatabaseService
    {
        public async void AddUser(string username, string password)
        {
            using (var db = new DatabaseContext())
            {
                Users user = new Users(username, password);
                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();
            }
        }

        public async void AddUser(Users user)
        {
            using (var db = new DatabaseContext())
            {
                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<Users>> GetUsers()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Users.ToListAsync();
            }
        }

        public async Task<Users> GetUser(int id)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Users.FindAsync(id);
            }
        }

        public async Task<Users> GetUser(string username, string password)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Users.FirstOrDefaultAsync(x => x.Username == username && x.Password == password);
            }
        }
    }
}