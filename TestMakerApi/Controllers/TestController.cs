using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private IDatabaseService _databaseService;
        private ITokenHandlerService _tokenHandlerService;
        private UserAuthorizationRequest _userHeader;

        #endregion Private Fields

        #region Public Constructors

        public TestController(IDatabaseService databaseService, ITokenHandlerService tokenHandlerService)
        {
            _databaseService = databaseService;
            _tokenHandlerService = tokenHandlerService;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Add test
        /// </summary>
        /// <param name="test">Test</param>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok() if added, ValidationProblem() if test is not valid, Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/addTest")]
        public async Task<ActionResult> AddTest(Test test, [FromHeader] string Authorization)
        {
            try
            {
                // why id and username into header?
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                if (IsTestValid(ref test))
                {
                    await _databaseService.AddTestAsync(test);
                    return Ok();
                }
                else
                    return ValidationProblem("Test is not valid");
            }
            // ??
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                return NotFound($"Database error: {e}");
            }
        }

        /// <summary>
        /// Get test list of user
        /// </summary>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok(Test list), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpGet("/test/getTestList")]
        public async Task<ActionResult<IList<Test>>> GetTestList([FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                var list = await _databaseService.GetTestListAsync(_userHeader.Username);
                return Ok(list);
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                return NotFound($"Database error: {e}");
            }
        }

        /// <summary>
        /// Get user's test by test id
        /// </summary>
        /// <param name="testId">Test id</param>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok(Test), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/getTest")]
        public async Task<ActionResult<Test>> GetTest([FromBody] int testId, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                var test = await _databaseService.GetTestAsync(testId);
                return Ok(test);
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                return NotFound($"Database error: {e}");
            }
        }

        /// <summary>
        /// Delete test
        /// </summary>
        /// <param name="test">Test</param>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok() if deleted, Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/deleteTest")]
        public async Task<ActionResult> DeleteTest(Test test, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                await _databaseService.DeleteTestAsync(test);
                return Ok("Deleted");
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                Debug.WriteLine(e);
                return NotFound($"Database error: {e}");
            }
        }

        /// <summary>
        /// Get a list of usernames who are allowed to take the test
        /// </summary>
        /// <param name="test">Test</param>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok(Usernames list), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/getTestAllowedUsers")]
        public async Task<ActionResult<IList<string>>> GetAllowedUsers(Test test, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                var users = await _databaseService.GetTestAllowedUsersAsync(test);
                return Ok(users);
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                return NotFound($"Database error: {e}");
            }
        }

        /// <summary>
        /// Allow the user to take the test
        /// </summary>
        /// <param name="testAccessRequest">Username and Test</param>
        /// <param name="Authorization"></param>
        /// <returns>Ok(), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/addTaskAllowedUser")]
        public async Task<ActionResult<IList<string>>> AddTaskAllowedUser(AddTestAccessRequest testAccessRequest, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                await _databaseService.AddTestAllowedUserAsync(testAccessRequest.Test, testAccessRequest.Username);
                return Ok("Added");
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                Debug.WriteLine(e);
                return NotFound($"Database error: {e}");
            }
        }

        /// <summary>
        /// Prevent the user from taking the test
        /// </summary>
        /// <param name="testAccessRequest"> Username and Test</param>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok(), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/deleteTaskAllowedUser")]
        public async Task<ActionResult<IList<string>>> DeleteTaskAllowedUser(AddTestAccessRequest testAccessRequest, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                await _databaseService.DeleteTestAllowedUserAsync(testAccessRequest.Test, testAccessRequest.Username);
                return Ok("Deleted");
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                Debug.WriteLine(e);
                return NotFound($"Database error: {e}");
            }
        }

        /// <summary>
        /// Update test
        /// </summary>
        /// <param name="test">Test</param>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok(Allowed users), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/updateTest")]
        public async Task<ActionResult<IList<string>>> UpdateTest(Test test, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                await _databaseService.UpdateTestAsync(test);
                return Ok("Updated");
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                Debug.WriteLine(e);
                return NotFound($"Database error: {e}");
            }
        }

        /// <summary>
        /// Get list of tests that user is allowed to take
        /// </summary>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok(list of tests), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpGet("/test/getAllowedTestList")]
        public async Task<ActionResult<IList<Test>>> GetAllowedTestList([FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                var testList = await _databaseService.GetAllowedTestList(_userHeader.Username);
                return Ok(testList);
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                Debug.WriteLine(e);
                return NotFound($"Database error: {e}");
            }
        }

        /// <summary>
        /// Get user's test result list (Where every test the user passes is the best)
        /// </summary>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok(list of test results), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpGet("/test/getUserTestResultList")]
        public async Task<ActionResult<IList<TestResult>>> GetUserTestResultList([FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                var list = await _databaseService.GetUserBestTestResultsList(_userHeader.Username);
                return Ok(list);
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                Debug.WriteLine(e);
                return NotFound($"Database error: {e}");
            }
        }

        /// <summary>
        /// Get best result of every user, that passed the test
        /// </summary>
        /// <param name="testId">Test id</param>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok(list of test results), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/getTestResultList")]
        public async Task<ActionResult<IList<TestResult>>> GetTestResultList([FromBody] int testId, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                var list = await _databaseService.GetBestTestResultsList(testId);
                return Ok(list);
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                Debug.WriteLine(e);
                return NotFound($"Database error: {e}");
            }
        }

        /// <summary>
        /// Add test result
        /// </summary>
        /// <param name="testResult">Test result</param>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok() if added, Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/addTestResult")]
        public async Task<ActionResult> AddTestResult(TestResult testResult, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                CalculateTestResult(ref testResult);
                await _databaseService.AddTestResultAsync(testResult, _userHeader.Username);
                return Ok("Added");
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                Debug.WriteLine(e);
                return NotFound($"Database error: {e}");
            }
        }

        /// <summary>
        /// Get test result
        /// </summary>
        /// <param name="testId"></param>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok(Test result), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/getTestResult")]
        public async Task<ActionResult<TestResult>> GetTestResult([FromBody] int testResultId, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                var testResult = await _databaseService.GetTestResultAsync(testResultId);
                return Ok(testResult);
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                Debug.WriteLine(e);
                return NotFound($"Database error: {e}");
            }
        }

        /// <summary>
        /// Is user can check test's right answers of test result
        /// </summary>
        /// <param name="testId">Test id</param>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns>Ok(true) if can, else Ok(false), Unauthorized() if token error, Not Found() if db error</returns>
        [HttpPost("/test/isUserCanCheckTestResult")]
        public async Task<ActionResult<bool>> IsUserCanCheckTestResult([FromBody] int testId, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                var testResult = await _databaseService.IsUserCanCheckTestResult(testId, _userHeader.Username);
                return Ok(testResult);
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                Debug.WriteLine(e);
                return NotFound($"Database error: {e}");
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