using System;
using System.Collections.Generic;
using System.Linq;
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

        public Task<User> GetUserAsync(int id);

        public Task<User> GetUserAsync(string username, string password);

        public Task<bool> IsUserExistsAsync(string username);

        public Task UpdateUserRefreshTokenAsync(User user, RefreshToken oldRefreshToken);

        public Task<RefreshToken> GetRefreshTokenAsync(int userId);

        public Task AddTestAsync(Test test);

        public Task<IList<Test>> GetTestListAsync(string username);

        public Task DeleteTestAsync(Test test);

        public Task<IList<string>> GetUsernamesAsync();

        public Task<IList<string>> GetTestAllowedUsersAsync(Test test);

        public Task AddTestAllowedUserAsync(Test test, string username);

        public Task DeleteTestAllowedUserAsync(Test test, string username);

        public Task<Test> GetTestAsync(int testId);

        public Task UpdateTestAsync(Test test);

        public Task<bool> IsTestExistsAsync(int id);

        public Task<IList<Test>> GetAllowedTestListAsync(string username);

        public Task AddTestResultAsync(TestResult test, string username);

        public Task<IList<TestResult>> GetUserBestTestResultsListAsync(string username);

        public Task<IList<TestResult>> GetBestTestResultsListAsync(int testId);

        public Task<TestResult> GetTestResultAsync(int testResultId);

        public Task<bool> IsUserCanCheckTestResultAsync(int testId, string username);
    }
}