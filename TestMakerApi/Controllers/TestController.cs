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
using TestMakerApi.Services;

namespace TestMakerApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        #region Private Fields

        private readonly ITestControllerService _testControllerService;

        #endregion Private Fields

        #region Public Constructors

        public TestController(ITestControllerService testControllerService)
        {
            _testControllerService = testControllerService;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Add test
        /// </summary>
        /// <param name="test">Test</param>
        [HttpPost("/test")]
        [Authorize]
        public async Task<ActionResult> AddTest(Test test)
        {
            try
            {
                if (_testControllerService.IsTestValid(ref test))
                {
                    await _testControllerService.AddTestAsync(test);
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
        [HttpGet("/test")]
        [Authorize]
        public async Task<ActionResult<IList<Test>>> GetTestList()
        {
            try
            {
                var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                var list = await _testControllerService.GetTestListAsync(username);
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
        [HttpPost("/test/{id}")]
        // review http request types
        [Authorize]
        public async Task<ActionResult<Test>> GetTest([FromQuery] id,[FromBody] int testId)
        {
            try
            {
                var test = await _testControllerService.GetTestAsync(testId);
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
        [HttpPost("/test/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteTest(Test test)
        {
            try
            {
                await _testControllerService.DeleteTestAsync(test);
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
        [HttpPost("/test/getTestAllowedUsers")]
        [Authorize]
        public async Task<ActionResult<IList<string>>> GetAllowedUsers(Test test)
        {
            try
            {
                var users = await _testControllerService.GetAllowedUsersAsync(test);
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
        [HttpPost("/test/addTestAllowedUser")]
        [Authorize]
        public async Task<ActionResult> AddTestAllowedUser(AddTestAccessRequest testAccessRequest)
        {
            try
            {
                await _testControllerService.AddTestAllowedUserAsync(testAccessRequest.Test, testAccessRequest.Username);
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
        [HttpPost("/test/deleteTestAllowedUser")]
        [Authorize]
        public async Task<ActionResult<IList<string>>> DeleteTestAllowedUser(AddTestAccessRequest testAccessRequest)
        {
            try
            {
                await _testControllerService.DeleteTestAllowedUserAsync(testAccessRequest.Test, testAccessRequest.Username);
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
        [HttpPut("/test/{id}")]
        [Authorize]
        // move Test to DTO
        public async Task<ActionResult<IList<string>>> UpdateTest([FromQuery] string id,Test test)
        {
            try
            {
                await _testControllerService.UpdateTestAsync(test);
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
        [HttpGet("/test/getAllowedTestList")]
        [Authorize]
        public async Task<ActionResult<IList<Test>>> GetAllowedTestList()
        {
            try
            {
                var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                var testList = await _testControllerService.GetAllowedTestListAsync(username);
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
        [HttpGet("/test/getUserTestResultList")]
        [Authorize]
        public async Task<ActionResult<IList<TestResult>>> GetUserTestResultList()
        {
            try
            {
                var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                var list = await _testControllerService.GetUserTestResultListAsync(username);
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
        [HttpPost("/test/getTestResultList")]
        [Authorize]
        public async Task<ActionResult<IList<TestResult>>> GetTestResultList([FromBody] int testId)
        {
            try
            {
                var list = await _testControllerService.GetTestResultListAsync(testId);
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
        [HttpPost("/test/addTestResult")]
        [Authorize]
        public async Task<ActionResult> AddTestResult(TestResult testResult)
        {
            try
            {
                var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                await _testControllerService.AddTestResultAsync(testResult, username);
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
        [HttpPost("/test/{id}/result")]
        [Authorize]
        public async Task<ActionResult<TestResult>> GetTestResult([FromBody] int testResultId)
        {
            try
            {
                var testResult = await _testControllerService.GetTestResultAsync(testResultId);
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
        [HttpPost("/test/isUserCanCheckTestResult")]
        [Authorize]
        public async Task<ActionResult<bool>> IsUserCanCheckTestResult([FromBody] int testId)
        {
            try
            {
                var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                var isCanCheck = await _testControllerService.IsUserCanCheckTestResultAsync(testId, username);
                return Ok(isCanCheck);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion Public Methods
    }
}