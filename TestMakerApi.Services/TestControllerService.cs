using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestMaker.Database.Entities;
using TestMaker.Database.Services;

namespace TestMakerApi.Services
{
    public class TestControllerService : ITestControllerService
    {
        #region Public Constructors

        public TestControllerService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Validate test
        /// </summary>
        /// <param name="test">Test</param>
        public bool IsTestValid(ref Test test)
        {
            if (test.Attempts == 0 || test.Questions.Count == 0 || string.IsNullOrWhiteSpace(test.Name))
                return false;
            foreach (var question in test.Questions)
            {
                if (string.IsNullOrWhiteSpace(question.Question) || question.Answers.Count < 2)
                    return false;
                int count = 0;
                foreach (var answer in question.Answers)
                {
                    if (string.IsNullOrWhiteSpace(answer.Answer))
                        return false;
                    if (answer.IsRight)
                        count++;
                }
                if (count == 0)
                    return false;
            }
            return true;
        }

        public async Task<bool> IsUserCanCheckTestResultAsync(int testId, string username)
        {
            var testResult = await _databaseService.IsUserCanCheckTestResultAsync(testId, username);
            return testResult;
        }

        public async Task<TestResult> GetTestResultAsync(int testResultId)
        {
            var testResult = await _databaseService.GetTestResultAsync(testResultId);
            return testResult;
        }

        public async Task AddTestResultAsync(TestResult testResult, string username)
        {
            CalculateTestResult(ref testResult);
            await _databaseService.AddTestResultAsync(testResult, username);
        }

        public async Task<IList<TestResult>> GetTestResultListAsync(int testId)
        {
            var list = await _databaseService.GetBestTestResultsListAsync(testId);
            return list;
        }

        public async Task<IList<TestResult>> GetUserTestResultListAsync(string username)
        {
            var list = await _databaseService.GetUserBestTestResultsListAsync(username);
            return list;
        }

        public async Task<IList<Test>> GetAllowedTestListAsync(string username)
        {
            var list = await _databaseService.GetAllowedTestListAsync(username);
            return list;
        }

        public async Task UpdateTestAsync(Test test)
        {
            await _databaseService.UpdateTestAsync(test);
        }

        public async Task DeleteTestAllowedUserAsync(Test test, string username)
        {
            await _databaseService.DeleteTestAllowedUserAsync(test, username);
        }

        public async Task AddTestAllowedUserAsync(Test test, string username)
        {
            await _databaseService.AddTestAllowedUserAsync(test, username);
        }

        public async Task<IList<string>> GetAllowedUsersAsync(Test test)
        {
            var users = await _databaseService.GetTestAllowedUsersAsync(test);
            return users;
        }

        public async Task DeleteTestAsync(Test test)
        {
            await _databaseService.DeleteTestAsync(test);
        }

        public async Task<Test> GetTestAsync(int testId)
        {
            var test = await _databaseService.GetTestAsync(testId);

            return test;
        }

        public async Task<IList<Test>> GetTestListAsync(string username)
        {
            var list = await _databaseService.GetTestListAsync(username);
            return list;
        }

        public async Task AddTestAsync(Test test)
        {
            await _databaseService.AddTestAsync(test);
        }

        #endregion Public Methods

        #region Private Fields

        private readonly IDatabaseService _databaseService;

        #endregion Private Fields

        #region Private Methods

        /// <summary>
        /// calculate result of test results
        /// </summary>
        private void CalculateTestResult(ref TestResult testResult)
        {
            int count;
            foreach (var question in testResult.Questions)
            {
                count = 0;
                foreach (var answer in question.Answers)
                {
                    if (answer.IsRight == answer.IsSelected)
                        count++;
                }
                if (count == question.Answers.Count)
                    testResult.Result++;
            }
            testResult.Result *= (float)100 / testResult.Questions.Count;
        }

        #endregion Private Methods
    }
}