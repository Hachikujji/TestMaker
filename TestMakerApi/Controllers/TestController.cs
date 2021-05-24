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

        [HttpPost("/test/addTest")]
        public async Task<ActionResult> AddTest(Test test, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }

            if (test.Attempts == 0)
                return ValidationProblem("Attempt error");
            if (string.IsNullOrWhiteSpace(test.Name))
                return ValidationProblem("Test name error");
            if (test.Questions.Count == 0)
                return ValidationProblem("Question count error");
            foreach (var question in test.Questions)
            {
                if (string.IsNullOrWhiteSpace(question.Question))
                    return ValidationProblem("Question name error");
                if (question.Answers.Count < 2)
                    return ValidationProblem("Answer count error");
                int count = 0;
                foreach (var answer in question.Answers)
                {
                    if (string.IsNullOrWhiteSpace(answer.Answer))
                        return ValidationProblem("Answer name error");
                    if (answer.IsRight)
                        count++;
                }
                if (count == 0)
                    return ValidationProblem("Answer error");
            }

            try
            {
                await _databaseService.AddTestAsync(test);
                return Ok();
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
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }

            IList<Test> list;
            try
            {
                list = await _databaseService.GetTestListAsync(_userHeader.Username);
                return Ok(list);
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
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            try
            {
                var test = await _databaseService.GetTestAsync(getTestRequest.TestId);
                return Ok(test);
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
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }
            try
            {
                var test = await _databaseService.GetTestAsync(getTestRequest.TestId);
                return Ok(test);
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
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }

            try
            {
                await _databaseService.DeleteTestAsync(test);
                return Ok("Deleted");
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
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }

            try
            {
                IList<string> users = await _databaseService.GetTestAllowedUsersAsync(test);
                return Ok(users);
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
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }

            try
            {
                await _databaseService.AddTestAllowedUserAsync(testAccessRequest.Test, testAccessRequest.Username);
                return Ok("Added");
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
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }

            try
            {
                await _databaseService.DeleteTestAllowedUserAsync(testAccessRequest.Test, testAccessRequest.Username);
                return Ok("Deleted");
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
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }

            try
            {
                await _databaseService.UpdateTestAsync(test);
                return Ok("Updated");
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
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }

            try
            {
                var testList = await _databaseService.GetAllowedTestList(_userHeader.Username);
                return Ok(testList);
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
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }

            try
            {
                var list = await _databaseService.GetUserBestTestResultsList(_userHeader.Username);
                return Ok(list);
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
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }

            try
            {
                var list = await _databaseService.GetBestTestResultsList(testRequest.TestId);
                return Ok(list);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                Debug.WriteLine(e);
                return NotFound($"Database error: {e}");
            }
        }

        [HttpPost("/test/sendTestResult")]
        public async Task<ActionResult> SendTestResult(TestResult testResult, [FromHeader] string Authorization)
        {
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return Unauthorized($"Token error: {e}");
            }

            try
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
                testResult.Result *= 100 / testResult.Questions.Count;
                await _databaseService.AddTestResultAsync(testResult, _userHeader.Username);
                return Ok("Added");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                Debug.WriteLine(e);
                return NotFound($"Database error: {e}");
            }
        }
    }
}