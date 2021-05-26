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
        private IDatabaseService _databaseService;
        private ITokenHandlerService _tokenHandlerService;
        private UserAuthorizationRequest _userHeader;

        public TestController(IDatabaseService databaseService, ITokenHandlerService tokenHandlerService)
        {
            _databaseService = databaseService;
            _tokenHandlerService = tokenHandlerService;
        }

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

        [HttpPost("/test/addTest")]
        public async Task<ActionResult> AddTest(Test test, [FromHeader] string Authorization)
        {
            try
            {
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
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                return NotFound($"Database error: {e}");
            }
        }

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

        [HttpPost("/test/getTest")]
        public async Task<ActionResult<Test>> GetTest(GetTestRequest getTestRequest, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                var test = await _databaseService.GetTestAsync(getTestRequest.TestId);
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

        [HttpPost("/test/getAttemptsLeft")]
        public async Task<ActionResult<Test>> GetAttemptsLeft(GetTestRequest getTestRequest, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                var test = await _databaseService.GetTestAsync(getTestRequest.TestId);
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

        [HttpPost("/test/getTestResultList")]
        public async Task<ActionResult<IList<TestResult>>> GetTestResultList(GetTestRequest testRequest, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                var list = await _databaseService.GetBestTestResultsList(testRequest.TestId);
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

        [HttpPost("/test/getTestResult")]
        public async Task<ActionResult<TestResult>> GetTestResult(GetTestRequest testRequest, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                var testResult = await _databaseService.GetTestResultAsync(testRequest.TestId);
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

        [HttpPost("/test/isUserCanCheckTestResult")]
        public async Task<ActionResult<bool>> IsUserCanCheckTestResult(GetTestRequest testRequest, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                var testResult = await _databaseService.IsUserCanCheckTestResult(testRequest.TestId, _userHeader.Username);
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
    }
}