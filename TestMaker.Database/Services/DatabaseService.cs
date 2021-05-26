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
        #region User

        /// <summary>
        /// Add new user async
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>Task</returns>
        public async Task AddUserAsync(string username, string password)
        {
            using (var db = new DatabaseContext())
            {
                User user = new User(username, password);
                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Add user async
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>Task</returns>
        public async Task AddUserAsync(User user)
        {
            using (var db = new DatabaseContext())
            {
                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Is user exists async
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>true if exists ,else returns false</returns>
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

        /// <summary>
        /// Get user async by id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>user</returns>
        public async Task<User> GetUserAsync(int id)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Users.FindAsync(id);
            }
        }

        /// <summary>
        /// Get user async by username and password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>user</returns>
        public async Task<User> GetUserAsync(string username, string password)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Users.FirstOrDefaultAsync(x => x.Username == username && x.Password == password);
            }
        }

        /// <summary>
        /// Get user async by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User</returns>
        private async Task<User> GetUserAsync(string username)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Users.FirstOrDefaultAsync(x => x.Username == username);
            }
        }

        /// <summary>
        /// Get list if all users async
        /// </summary>
        /// <returns>List of users</returns>
        public async Task<IList<string>> GetUsernamesAsync()
        {
            using (var db = new DatabaseContext())
            {
                var list = await db.Users.Select(u => u.Username).ToListAsync();
                return list;
            }
        }

        /// <summary>
        /// Update user's refresh token async
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="refreshToken">New refresh token</param>
        /// <returns>Task</returns>
        public async Task UpdateUserRefreshTokenAsync(User user, RefreshToken refreshToken)
        {
            using (var db = new DatabaseContext())
            {
                db.Attach(user);
                user.RefreshToken = refreshToken;
                db.Users.Update(user);
                await db.SaveChangesAsync();
            }
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
            using (var db = new DatabaseContext())
            {
                var token = await db.RefreshTokens.Where(token => EF.Property<int?>(token, "UserId") == userId).SingleAsync();
                return token;
            }
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
            using (var db = new DatabaseContext())
            {
                await db.Test.AddAsync(test);
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Get lsit of user's tests async by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>List of tests</returns>
        public async Task<IList<Test>> GetTestListAsync(string username)
        {
            using (var db = new DatabaseContext())
            {
                var testList = await db.Test.Where(test => test.CreatorName == username).Include(p => ((Test)p).Questions).ThenInclude(q => ((TestQuestion)q).Answers).ToListAsync();
                return testList;
            }
        }

        /// <summary>
        /// Delete test async
        /// </summary>
        /// <param name="test">Test</param>
        /// <returns>Task</returns>
        public async Task DeleteTestAsync(Test test)
        {
            using (var db = new DatabaseContext())
            {
                db.Attach(test);
                foreach (var question in test.Questions)
                    db.RemoveRange(question.Answers);
                db.RemoveRange(test.Questions);

                var deleteAccessList = await db.TestAccess.Where(a => a.Test.Id == test.Id).ToListAsync();
                db.RemoveRange(deleteAccessList);

                var deleteResultList = await db.TestResult.Where(tr => tr.Test.Id == test.Id).Include(tr => ((TestResult)tr).Questions).ThenInclude(q => ((TestResultQuestion)q).Answers).ToListAsync();
                foreach (var testResult in deleteResultList)
                {
                    foreach (var question in testResult.Questions)
                        db.RemoveRange(question.Answers);
                    db.RemoveRange(testResult.Questions);
                }
                db.RemoveRange(deleteResultList);

                db.Remove(test);
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Get username list of all users that can take the test
        /// </summary>
        /// <param name="test">Test</param>
        /// <returns>List of usernames</returns>
        public async Task<IList<string>> GetTestAllowedUsersAsync(Test test)
        {
            using (var db = new DatabaseContext())
            {
                var list = await db.TestAccess.Where(item => item.Test == test).Select(i => i.User.Username).ToListAsync();
                return list;
            }
        }

        /// <summary>
        /// Add new user's access to test
        /// </summary>
        /// <param name="test">Test</param>
        /// <param name="username">Username</param>
        /// <returns>Task</returns>
        public async Task AddTestAllowedUserAsync(Test test, string username)
        {
            using (var db = new DatabaseContext())
            {
                var user = await GetUserAsync(username);
                var newAccess = new TestAccess(user, test);
                db.Attach(user);
                db.Attach(test);
                if (await db.TestAccess.Where(t => t.User == user && t.Test == test).SingleOrDefaultAsync() == null)
                {
                    await db.TestAccess.AddAsync(newAccess);
                    await db.SaveChangesAsync();
                }
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
            using (var db = new DatabaseContext())
            {
                var user = await GetUserAsync(username);
                TestAccess testAccess;
                db.Attach(user);
                db.Attach(test);
                if ((testAccess = await db.TestAccess.Where(t => t.User == user && t.Test == test).SingleOrDefaultAsync()) != null)
                {
                    db.TestAccess.Remove(testAccess);
                    await db.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Get test async by id
        /// </summary>
        /// <param name="testId">Test id</param>
        /// <returns>Test</returns>
        public async Task<Test> GetTestAsync(int testId)
        {
            using (var db = new DatabaseContext())
            {
                var test = await db.Test.FindAsync(testId);
                try
                {
                    var a = db.Test.Include(t => ((Test)t).Questions).ThenInclude(q => ((TestQuestion)q).Answers).FirstOrDefault(t => t.Id == testId);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
                return test;
            }
        }

        /// <summary>
        /// Update test async
        /// </summary>
        /// <param name="test">Test</param>
        /// <returns>Task</returns>
        public async Task UpdateTestAsync(Test test)
        {
            using (var db = new DatabaseContext())
            {
                db.Update(test);
                var childs = await db.TestQuestion.Where(question => EF.Property<int?>(question, "TestId") == test.Id).Include(question => question.Answers).AsNoTracking().ToListAsync();
                foreach (var testQuestion in childs)
                {
                    var question = test.Questions.Where(q => q.Id == testQuestion.Id).SingleOrDefault();
                    if (question == null)
                        db.Remove(testQuestion);
                    else
                        db.Update(question);
                }

                await db.SaveChangesAsync();

                var deleteAnswers = await db.TestAnswer.Where(answer => EF.Property<int?>(answer, "TestQuestionId") == null).ToListAsync();
                foreach (var answer in deleteAnswers)
                    db.Remove(answer);
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Is test exists async by id
        /// </summary>
        /// <param name="id">Test id</param>
        /// <returns>true if exists, else returns false</returns>
        public async Task<bool> IsTestExistsAsync(int id)
        {
            using (var db = new DatabaseContext())
            {
                var test = await db.Test.Where(t => t.Id == id).SingleOrDefaultAsync();
                if (test != null)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Get all allowed user's tests that he can take
        /// </summary>
        /// <param name="username">username</param>
        /// <returns>list of tests</returns>
        public async Task<IList<Test>> GetAllowedTestList(string username)
        {
            using (var db = new DatabaseContext())
            {
                var testList = await db.Test.Where(t => db.TestAccess.Any(ta => ta.Test.Id == t.Id && ta.User.Username == username)).Include(t => ((Test)t).Questions).ThenInclude(q => ((TestQuestion)q).Answers).ToListAsync();
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
        }

        /// <summary>
        /// Get amount of test attempts left async
        /// </summary>
        /// <param name="test">Test</param>
        /// <param name="username">Username</param>
        /// <returns>int amount</returns>
        private async Task<int> GetTestAttemptsLeftAsync(Test test, string username)
        {
            using (var db = new DatabaseContext())
            {
                var AttemptCount = (await db.TestResult.Where(ta => ta.User.Username == username && ta.Test.Id == test.Id).ToListAsync()).Count;
                return AttemptCount;
            }
        }

        /// <summary>
        /// Add test result async
        /// </summary>
        /// <param name="testResult">Test result</param>
        /// <param name="username">Username</param>
        /// <returns>Task</returns>
        public async Task AddTestResultAsync(TestResult testResult, string username)
        {
            using (var db = new DatabaseContext())
            {
                User user = await GetUserAsync(username);
                testResult.User = user;
                db.Attach(testResult.Test);
                db.Attach(user);
                db.TestResult.Add(testResult);
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Get user best test results list
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>List of test results</returns>
        public async Task<IList<TestResult>> GetUserBestTestResultsList(string username)
        {
            using (var db = new DatabaseContext())
            {
                var listOfResults = await db.TestResult.Where(tr => tr.User.Username == username).Include(tr => tr.Test).ToListAsync();
                List<TestResult> listofBestResults = new List<TestResult>(listOfResults.GroupBy(mr => mr.Test).Select(grp => grp.OrderByDescending(mr => mr.Result).First()));

                return listofBestResults;
            }
        }

        /// <summary>
        /// Get best test results of test by test id
        /// </summary>
        /// <param name="testId">test id</param>
        /// <returns>list of test results</returns>
        public async Task<IList<TestResult>> GetBestTestResultsList(int testId)
        {
            using (var db = new DatabaseContext())
            {
                var listOfResults = await db.TestResult.Where(tr => tr.Test.Id == testId).Include(tr => tr.User).ToListAsync();
                List<TestResult> listofBestResults = new List<TestResult>(listOfResults.GroupBy(mr => mr.User).Select(grp => grp.OrderByDescending(mr => mr.Result).First()));

                return listofBestResults;
            }
        }

        /// <summary>
        /// Get test result by test result id
        /// </summary>
        /// <param name="testResultId">Test result id</param>
        /// <returns>Test result</returns>
        public async Task<TestResult> GetTestResultAsync(int testResultId)
        {
            using (var db = new DatabaseContext())
            {
                var testResult = await db.TestResult.Where(tr => ((TestResult)tr).Id == testResultId).Include(tr => ((TestResult)tr).Questions).ThenInclude(q => ((TestResultQuestion)q).Answers).SingleOrDefaultAsync();
                return testResult;
            }
        }

        /// <summary>
        /// Is user can check right answers of test results by test id
        /// </summary>
        /// <param name="testId">Test id</param>
        /// <param name="username">username</param>
        /// <returns>true if user can check, else returns false</returns>
        public async Task<bool> IsUserCanCheckTestResult(int testId, string username)
        {
            using (var db = new DatabaseContext())
            {
                var testResultCount = await db.TestResult.Where(tr => tr.Test.Id == testId && tr.User.Username == username).CountAsync();
                var test = await db.Test.FindAsync(testId);
                if (testResultCount == test.Attempts)
                    return true;
                else
                    return false;
            }
        }

        #endregion Test
    }
}