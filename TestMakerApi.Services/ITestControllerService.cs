using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestMaker.Database.Entities;

namespace TestMakerApi.Services
{
    public interface ITestControllerService
    {
        public Task<bool> IsUserCanCheckTestResultAsync(int testId, string username);

        public Task<TestResult> GetTestResultAsync(int testResultId);

        public Task AddTestResultAsync(TestResult testResult, string username);

        public Task<IList<TestResult>> GetTestResultListAsync(int testId);

        public Task<IList<TestResult>> GetUserTestResultListAsync(string username);

        public Task<IList<Test>> GetAllowedTestListAsync(string username);

        public Task UpdateTestAsync(Test test);

        public Task DeleteTestAllowedUserAsync(Test test, string username);

        public Task AddTestAllowedUserAsync(Test test, string username);

        public Task<IList<string>> GetAllowedUsersAsync(Test test);

        public Task DeleteTestAsync(Test test);

        public Task<Test> GetTestAsync(int testId);

        public Task<IList<Test>> GetTestListAsync(string username);

        public bool IsTestValid(ref Test test);

        public Task AddTestAsync(Test test);
    }
}