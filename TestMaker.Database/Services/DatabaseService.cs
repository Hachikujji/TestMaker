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

        public async Task<IList<User>> GetUsersAsync()
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

        private async Task<User> GetUserAsync(string username)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Users.FirstOrDefaultAsync(x => x.Username == username);
            }
        }

        public async Task<IList<string>> GetUsernamesAsync()
        {
            using (var db = new DatabaseContext())
            {
                var list = await db.Users.Select(u => u.Username).ToListAsync();
                return list;
            }
        }

        public async Task UpdateUserAsync(User user, RefreshToken refreshToken)
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

        public async Task AddTestAsync(Test test)
        {
            using (var db = new DatabaseContext())
            {
                await db.Test.AddAsync(test);
                await db.SaveChangesAsync();
            }
        }

        public async Task<IList<Test>> GetTestListAsync(string username)
        {
            using (var db = new DatabaseContext())
            {
                var testList = await db.Test.Where(test => test.CreatorName == username).Include(p => ((Test)p).Questions).ThenInclude(q => ((TestQuestion)q).Answers).ToListAsync();
                return testList;
            }
        }

        public async Task DeleteTestAsync(Test test)
        {
            using (var db = new DatabaseContext())
            {
                db.Attach(test);
                foreach (var question in test.Questions)
                {
                    foreach (var answer in question.Answers)
                        db.Remove(answer);
                    db.Remove(question);
                }
                var deleteAccessList = await db.TestAccess.Where(a => a.Test.Id == test.Id).ToListAsync();
                foreach (var access in deleteAccessList)
                {
                    db.Remove(access);
                }
                db.Remove(test);
                await db.SaveChangesAsync();
            }
        }

        public async Task<IList<string>> GetTestAllowedUsersAsync(Test test)
        {
            using (var db = new DatabaseContext())
            {
                var list = await db.TestAccess.Where(item => item.Test == test).Select(i => i.User.Username).ToListAsync();
                return list;
            }
        }

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

        private async Task<int> GetTestAttemptsLeftAsync(Test test, string username)
        {
            using (var db = new DatabaseContext())
            {
                var AttemptCount = (await db.TestResult.Where(ta => ta.User.Username == username && ta.Test.Id == test.Id).ToListAsync()).Count;
                return AttemptCount;
            }
        }

        public async Task AddTestResultAsync(TestResult test, string username)
        {
            using (var db = new DatabaseContext())
            {
                User user = await GetUserAsync(username);
                test.User = user;
                db.Attach(test.Test);
                db.Attach(user);
                db.TestResult.Add(test);
                await db.SaveChangesAsync();
            }
        }

        public async Task<IList<TestResult>> GetUserBestTestResultsList(string username)
        {
            using (var db = new DatabaseContext())
            {
                var listOfResults = await db.TestResult.Where(tr => tr.User.Username == username).Include(tr => tr.Test).ToListAsync();
                List<TestResult> listofBestResults = new List<TestResult>(listOfResults.GroupBy(mr => mr.Test).Select(grp => grp.OrderByDescending(mr => mr.Result).First()));

                return listofBestResults;
            }
        }
        public async Task<IList<TestResult>> GetBestTestResultsList(int testId)
        {
            using (var db = new DatabaseContext())
            {
                var listOfResults = await db.TestResult.Where(tr => tr.Test.Id == testId).Include(tr => tr.User).ToListAsync();
                List<TestResult> listofBestResults = new List<TestResult>(listOfResults.GroupBy(mr => mr.User).Select(grp => grp.OrderByDescending(mr => mr.Result).First()));

                return listofBestResults;
            }
        }

        #endregion Test
    }
}