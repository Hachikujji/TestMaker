using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly DatabaseContext _dbContext;

        public DatabaseService(DatabaseContext databaseContext)
        {
            _dbContext = databaseContext;
        }

        #region User

        /// <summary>
        /// Add new user async
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>Task</returns>
        public async Task AddUserAsync(string username, string password)
        {
            User user = new User(username, password);
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Add user async
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>Task</returns>
        public async Task AddUserAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Is user exists async
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>true if exists ,else returns false</returns>
        public async Task<bool> IsUserExistsAsync(string username)
        {
            var u = await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == username);
            if (u == null)
                return false;
            return true;
        }

        /// <summary>
        /// Get user async by id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>user</returns>
        public async Task<User> GetUserAsync(int id)
        {
            return await _dbContext.Users.FindAsync(id);
        }

        /// <summary>
        /// Get user async by username and password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>user</returns>
        public async Task<User> GetUserAsync(string username, string password)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == username && x.Password == password);
        }

        /// <summary>
        /// Get user async by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User</returns>
        private async Task<User> GetUserAsync(string username)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == username);
        }

        /// <summary>
        /// Get list if all users async
        /// </summary>
        /// <returns>List of users</returns>
        public async Task<IList<string>> GetUsernamesAsync()
        {
            var list = await _dbContext.Users.Select(u => u.Username).ToListAsync();
            return list;
        }

        /// <summary>
        /// Update user's refresh token async
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="refreshToken">New refresh token</param>
        /// <returns>Task</returns>
        public async Task UpdateUserRefreshTokenAsync(User user, RefreshToken refreshToken)
        {
            _dbContext.Attach(user);
            user.RefreshToken = refreshToken;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        #endregion User

        #region Token

        /// <summary>
        /// Get refresh token of user by id
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns> Refresh token</returns>
        public async Task<RefreshToken> GetRefreshTokenAsync(int userId)
        {
            var token = await _dbContext.RefreshTokens.Where(token => EF.Property<int?>(token, "UserId") == userId).SingleAsync();
            return token;
        }

        #endregion Token

        #region Test

        /// <summary>
        /// Add test async
        /// </summary>
        /// <param name="test">Test</param>
        /// <returns>Task</returns>
        public async Task AddTestAsync(Test test)
        {
            await _dbContext.Test.AddAsync(test);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Get lsit of user's tests async by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>List of tests</returns>
        public async Task<IList<Test>> GetTestListAsync(string username)
        {
            var testList = await _dbContext.Test.Where(test => test.CreatorName == username).Include(p => ((Test)p).Questions).ThenInclude(q => ((TestQuestion)q).Answers).ToListAsync();
            return testList;
        }

        /// <summary>
        /// Delete test async
        /// </summary>
        /// <param name="test">Test</param>
        /// <returns>Task</returns>
        public async Task DeleteTestAsync(Test test)
        {
            _dbContext.Attach(test);
            foreach (var question in test.Questions)
                _dbContext.RemoveRange(question.Answers);
            _dbContext.RemoveRange(test.Questions);

            var deleteAccessList = await _dbContext.TestAccess.Where(a => a.Test.Id == test.Id).ToListAsync();
            _dbContext.RemoveRange(deleteAccessList);

            var deleteResultList = await _dbContext.TestResult.Where(tr => tr.Test.Id == test.Id).Include(tr => ((TestResult)tr).Questions).ThenInclude(q => ((TestResultQuestion)q).Answers).ToListAsync();
            foreach (var testResult in deleteResultList)
            {
                foreach (var question in testResult.Questions)
                    _dbContext.RemoveRange(question.Answers);
                _dbContext.RemoveRange(testResult.Questions);
            }
            _dbContext.RemoveRange(deleteResultList);

            _dbContext.Remove(test);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Get username list of all users that can take the test
        /// </summary>
        /// <param name="test">Test</param>
        /// <returns>List of usernames</returns>
        public async Task<IList<string>> GetTestAllowedUsersAsync(Test test)
        {
            var list = await _dbContext.TestAccess.Where(item => item.Test == test).Select(i => i.User.Username).ToListAsync();
            return list;
        }

        /// <summary>
        /// Add new user's access to test
        /// </summary>
        /// <param name="test">Test</param>
        /// <param name="username">Username</param>
        /// <returns>Task</returns>
        public async Task AddTestAllowedUserAsync(Test test, string username)
        {
            var user = await GetUserAsync(username);
            var newAccess = new TestAccess(user, test);
            _dbContext.Attach(user);
            _dbContext.Attach(test);
            if (await _dbContext.TestAccess.Where(t => t.User == user && t.Test == test).SingleOrDefaultAsync() == null)
            {
                await _dbContext.TestAccess.AddAsync(newAccess);
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Delete user's access to test
        /// </summary>
        /// <param name="test">Test</param>
        /// <param name="username">Username</param>
        /// <returns>Task</returns>
        public async Task DeleteTestAllowedUserAsync(Test test, string username)
        {
            var user = await GetUserAsync(username);
            TestAccess testAccess;
            _dbContext.Attach(user);
            _dbContext.Attach(test);
            if ((testAccess = await _dbContext.TestAccess.Where(t => t.User == user && t.Test == test).SingleOrDefaultAsync()) != null)
            {
                _dbContext.TestAccess.Remove(testAccess);
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Get test async by id
        /// </summary>
        /// <param name="testId">Test id</param>
        /// <returns>Test</returns>
        public async Task<Test> GetTestAsync(int testId)
        {
            var test = _dbContext.Test.Include(t => ((Test)t).Questions).ThenInclude(q => ((TestQuestion)q).Answers).FirstOrDefault(t => t.Id == testId);
            return test;
        }

        /// <summary>
        /// Update test async
        /// </summary>
        /// <param name="test">Test</param>
        /// <returns>Task</returns>
        public async Task UpdateTestAsync(Test test)
        {
            _dbContext.Update(test);
            var childs = await _dbContext.TestQuestion.Where(question => EF.Property<int?>(question, "TestId") == test.Id).Include(question => question.Answers).AsNoTracking().ToListAsync();
            foreach (var testQuestion in childs)
            {
                var question = test.Questions.Where(q => q.Id == testQuestion.Id).SingleOrDefault();
                if (question == null)
                    _dbContext.Remove(testQuestion);
                else
                    _dbContext.Update(question);
            }

            await _dbContext.SaveChangesAsync();

            var deleteAnswers = await _dbContext.TestAnswer.Where(answer => EF.Property<int?>(answer, "TestQuestionId") == null).ToListAsync();
            foreach (var answer in deleteAnswers)
                _dbContext.Remove(answer);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Is test exists async by id
        /// </summary>
        /// <param name="id">Test id</param>
        /// <returns>true if exists, else returns false</returns>
        public async Task<bool> IsTestExistsAsync(int id)
        {
            var test = await _dbContext.Test.Where(t => t.Id == id).SingleOrDefaultAsync();
            return test != null;
        }

        /// <summary>
        /// Get all allowed user's tests that he can take
        /// </summary>
        /// <param name="username">username</param>
        /// <returns>list of tests</returns>
        public async Task<IList<Test>> GetAllowedTestList(string username)
        {
            var testList = await _dbContext.Test.Where(t => _dbContext.TestAccess.Any(ta => ta.Test.Id == t.Id && ta.User.Username == username)).Include(t => ((Test)t).Questions).ThenInclude(q => ((TestQuestion)q).Answers).ToListAsync();
            int testCount;
            List<Test> deleteList = new List<Test>();
            foreach (var test in testList)
            {
                testCount = await GetTestAttemptsLeftAsync(test, username);
                if (testCount >= test.Attempts)
                    deleteList.Add(test);
                else
                    test.Attempts -= testCount;
            }
            foreach (var delTest in deleteList)
            {
                testList.Remove(delTest);
            }
            return testList;
        }

        /// <summary>
        /// Get amount of test attempts left async
        /// </summary>
        /// <param name="test">Test</param>
        /// <param name="username">Username</param>
        /// <returns>int amount</returns>
        private async Task<int> GetTestAttemptsLeftAsync(Test test, string username)
        {
            var AttemptCount = (await _dbContext.TestResult.Where(ta => ta.User.Username == username && ta.Test.Id == test.Id).ToListAsync()).Count;
            return AttemptCount;
        }

        /// <summary>
        /// Add test result async
        /// </summary>
        /// <param name="testResult">Test result</param>
        /// <param name="username">Username</param>
        /// <returns>Task</returns>
        public async Task AddTestResultAsync(TestResult testResult, string username)
        {
            User user = await GetUserAsync(username);
            testResult.User = user;
            _dbContext.Attach(testResult.Test);
            _dbContext.Attach(user);
            _dbContext.TestResult.Add(testResult);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Get user best test results list
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>List of test results</returns>
        public async Task<IList<TestResult>> GetUserBestTestResultsList(string username)
        {
            var listOfResults = await _dbContext.TestResult.Where(tr => tr.User.Username == username).Include(tr => tr.Test).ToListAsync();
            List<TestResult> listofBestResults = new List<TestResult>(listOfResults.GroupBy(mr => mr.Test).Select(grp => grp.OrderByDescending(mr => mr.Result).First()));

            return listofBestResults;
        }

        /// <summary>
        /// Get best test results of test by test id
        /// </summary>
        /// <param name="testId">test id</param>
        /// <returns>list of test results</returns>
        public async Task<IList<TestResult>> GetBestTestResultsList(int testId)
        {
            var listOfResults = await _dbContext.TestResult.Where(tr => tr.Test.Id == testId).Include(tr => tr.User).ToListAsync();
            List<TestResult> listofBestResults = new List<TestResult>(listOfResults.GroupBy(mr => mr.User).Select(grp => grp.OrderByDescending(mr => mr.Result).First()));

            return listofBestResults;
        }

        /// <summary>
        /// Get test result by test result id
        /// </summary>
        /// <param name="testResultId">Test result id</param>
        /// <returns>Test result</returns>
        public async Task<TestResult> GetTestResultAsync(int testResultId)
        {
            var testResult = await _dbContext.TestResult.Where(tr => ((TestResult)tr).Id == testResultId).Include(tr => ((TestResult)tr).Questions).ThenInclude(q => ((TestResultQuestion)q).Answers).SingleOrDefaultAsync();
            return testResult;
        }

        /// <summary>
        /// Is user can check right answers of test results by test id
        /// </summary>
        /// <param name="testId">Test id</param>
        /// <param name="username">username</param>
        /// <returns>true if user can check, else returns false</returns>
        public async Task<bool> IsUserCanCheckTestResult(int testId, string username)
        {
            var testResultCount = await _dbContext.TestResult.Where(tr => tr.Test.Id == testId && tr.User.Username == username).CountAsync();
            var test = await _dbContext.Test.FindAsync(testId);
            if (testResultCount == test.Attempts)
                return true;
            else
                return false;
        }

        #endregion Test
    }
}