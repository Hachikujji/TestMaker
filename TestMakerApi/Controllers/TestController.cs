using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TestMaker.Database.Entities;
using TestMaker.Database.Models;
using TestMaker.Database.Services;
using TestMakerApi.Services;

namespace TestMakerApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        #region Private Fields

        private readonly IDatabaseService _databaseService;

        #endregion Private Fields

        #region Public Constructors

        public TestController(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Add test
        /// </summary>
        /// <param name="test">Test</param>
        /// <returns>Ok() if added, ValidationProblem() if test is not valid, Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/addTest")]
        [Authorize]
        public async Task<ActionResult> AddTest(Test test)
        {
            try
            {
                if (IsTestValid(ref test))
                {
                    await _databaseService.AddTestAsync(test);
                    return Ok();
                }
                else
                    return ValidationProblem("Test is not valid");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get test list of user
        /// </summary>
        /// <returns>Ok(Test list), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpGet("/test/getTestList")]
        [Authorize]
        public async Task<ActionResult<IList<Test>>> GetTestList()
        {
            try
            {
                var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                var list = await _databaseService.GetTestListAsync(username);
                return Ok(list);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get user's test by test id
        /// </summary>
        /// <param name="testId">Test id</param>
        /// <returns>Ok(Test), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/getTest")]
        [Authorize]
        public async Task<ActionResult<Test>> GetTest([FromBody] int testId)
        {
            try
            {
                var test = await _databaseService.GetTestAsync(testId);
                return Ok(test);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Delete test
        /// </summary>
        /// <param name="test">Test</param>
        /// <returns>Ok() if deleted, Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/deleteTest")]
        [Authorize]
        public async Task<ActionResult> DeleteTest(Test test)
        {
            try
            {
                await _databaseService.DeleteTestAsync(test);
                return Ok("Deleted");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get a list of usernames who are allowed to take the test
        /// </summary>
        /// <param name="test">Test</param>
        /// <returns>Ok(Usernames list), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/getTestAllowedUsers")]
        [Authorize]
        public async Task<ActionResult<IList<string>>> GetAllowedUsers(Test test)
        {
            try
            {
                var users = await _databaseService.GetTestAllowedUsersAsync(test);
                return Ok(users);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Allow the user to take the test
        /// </summary>
        /// <param name="testAccessRequest">Username and Test</param>
        /// <returns>Ok(), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/addTaskAllowedUser")]
        [Authorize]
        public async Task<ActionResult<IList<string>>> AddTaskAllowedUser(AddTestAccessRequest testAccessRequest)
        {
            try
            {
                await _databaseService.AddTestAllowedUserAsync(testAccessRequest.Test, testAccessRequest.Username);
                return Ok("Added");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Prevent the user from taking the test
        /// </summary>
        /// <param name="testAccessRequest"> Username and Test</param>
        /// <returns>Ok(), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/deleteTaskAllowedUser")]
        [Authorize]
        public async Task<ActionResult<IList<string>>> DeleteTaskAllowedUser(AddTestAccessRequest testAccessRequest)
        {
            try
            {
                await _databaseService.DeleteTestAllowedUserAsync(testAccessRequest.Test, testAccessRequest.Username);
                return Ok("Deleted");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Update test
        /// </summary>
        /// <param name="test">Test</param>
        /// <returns>Ok(Allowed users), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/updateTest")]
        [Authorize]
        public async Task<ActionResult<IList<string>>> UpdateTest(Test test)
        {
            try
            {
                await _databaseService.UpdateTestAsync(test);
                return Ok("Updated");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get list of tests that user is allowed to take
        /// </summary>
        /// <returns>Ok(list of tests), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpGet("/test/getAllowedTestList")]
        [Authorize]
        public async Task<ActionResult<IList<Test>>> GetAllowedTestList()
        {
            try
            {
                var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                var testList = await _databaseService.GetAllowedTestList(username);
                return Ok(testList);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get user's test result list (Where every test the user passes is the best)
        /// </summary>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok(list of test results), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpGet("/test/getUserTestResultList")]
        [Authorize]
        public async Task<ActionResult<IList<TestResult>>> GetUserTestResultList()
        {
            try
            {
                var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                var list = await _databaseService.GetUserBestTestResultsList(username);
                return Ok(list);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get best result of every user, that passed the test
        /// </summary>
        /// <param name="testId">Test id</param>
        /// <returns>Ok(list of test results), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/getTestResultList")]
        [Authorize]
        public async Task<ActionResult<IList<TestResult>>> GetTestResultList([FromBody] int testId)
        {
            try
            {
                var list = await _databaseService.GetBestTestResultsList(testId);
                return Ok(list);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Add test result
        /// </summary>
        /// <param name="testResult">Test result</param>
        /// <returns>Ok() if added, Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/addTestResult")]
        [Authorize]
        public async Task<ActionResult> AddTestResult(TestResult testResult)
        {
            try
            {
                var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                CalculateTestResult(ref testResult);
                await _databaseService.AddTestResultAsync(testResult, username);
                return Ok("Added");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get test result
        /// </summary>
        /// <param name="testId"></param>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok(Test result), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/getTestResult")]
        [Authorize]
        public async Task<ActionResult<TestResult>> GetTestResult([FromBody] int testResultId)
        {
            try
            {
                var testResult = await _databaseService.GetTestResultAsync(testResultId);
                return Ok(testResult);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Is user can check test's right answers of test result
        /// </summary>
        /// <param name="testId">Test id</param>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok(true) if can, else Ok(false), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/isUserCanCheckTestResult")]
        [Authorize]
        public async Task<ActionResult<bool>> IsUserCanCheckTestResult([FromBody] int testId)
        {
            try
            {
                var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                var testResult = await _databaseService.IsUserCanCheckTestResult(testId, username);
                return Ok(testResult);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Validate test
        /// </summary>
        /// <param name="test">Test</param>
        /// <returns>true if test is valid, else returns false</returns>
        private bool IsTestValid(ref Test test)
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

        /// <summary>
        /// calculate result of test results
        /// </summary>
        /// <param name="testResult">Test result class</param>
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