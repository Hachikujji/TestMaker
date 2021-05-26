using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TestMaker.Database;
using TestMaker.Database.Entities;
using TestMaker.Database.Models;
using TestMaker.Database.Services;
using TestMakerApi.Services;

namespace TestMakerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthorizationController : Controller
    {
        private IDatabaseService _databaseService;
        private ITokenHandlerService _tokenHandlerService;

        public AuthorizationController(IDatabaseService databaseService, ITokenHandlerService tokenHandlerService)
        {
            _databaseService = databaseService;
            _tokenHandlerService = tokenHandlerService;
        }

        [HttpPost("/user/addUser")]
        public async Task<ActionResult<User>> AddUsers(UserAuthenticationRequest userInfo)
        {
            if (!string.IsNullOrWhiteSpace(userInfo.Username) && !string.IsNullOrWhiteSpace(userInfo.Password))
            {
                var user = new User(userInfo.Username, userInfo.Password);
                try
                {
                    await _databaseService.AddUserAsync(user);
                    return Ok(user);
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
                {
                    return NotFound($"Database error: {e}");
                }
            }
            else
                return BadRequest(new { errorText = "Invalid username or password." });
        }

        [HttpPost("/user/authorization")]
        public async Task<ActionResult<UserAuthenticationResponse>> Authorization(UserAuthenticationRequest model)
        {
            User user;
            try
            {
                user = await _databaseService.GetUserAsync(model.Username, model.Password);
                if (user == null)
                    return BadRequest(new { errorText = "Invalid username or password." });
                var jwtToken = _tokenHandlerService.CreateJwtToken();
                var refreshToken = _tokenHandlerService.CreateRefreshToken();
                await _databaseService.UpdateUserAsync(user, refreshToken);
                return Ok(new UserAuthenticationResponse(user, jwtToken, refreshToken.Token));
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                return NotFound($"Database error: {e}");
            }
        }

        [HttpGet("/user/validateToken")]
        public ActionResult ValidateToken([FromHeader] string Authorization)
        {
            if (string.IsNullOrWhiteSpace(Authorization))
                return BadRequest("Token not provided");
            UserAuthorizationRequest userHeader;
            try
            {
                userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return BadRequest($"Wrong authorization header: {e}");
            }

            if (userHeader == null || !_tokenHandlerService.ValidateToken(userHeader.JwtToken))
                return Unauthorized("Token is not valid");
            return Ok("Token is valid");
        }

        //[HttpGet("/user/getUser/{id}")]
        //public async Task<ActionResult<User>> GetUserById(int id, [FromHeader] string Authorization)
        //{
        //    UserAuthorizationRequest userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
        //    if (!_tokenHandlerService.ValidateToken(userHeader.JwtToken))
        //        return Unauthorized("Token error");
        //    User user;
        //    try
        //    {
        //        user = await _databaseService.GetUserAsync(id);
        //        if (user == null)
        //            return NotFound();
        //        return user;
        //    }
        //    catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
        //    {
        //        return NotFound($"Database error: {e}");
        //    }
        //}

        [HttpGet("/user/isUserExists/{username}")]
        public async Task<ActionResult<User>> IsUserExists(string username)
        {
            bool isUserExists;
            try
            {
                isUserExists = await _databaseService.IsUserExistsAsync(username);
                if (isUserExists == false)
                    return NotFound(isUserExists);
                return Ok(isUserExists);
            }
            catch (Exception e)
            {
                return NotFound($"Database error: {e}");
            }
        }

        [HttpPost("/user/refreshToken")]
        public async Task<ActionResult<UserAuthenticationResponse>> RefreshToken(UserAuthenticationResponse response)
        {
            try
            {
                var user = await _databaseService.GetUserAsync(response.Id);
                if (response.RefreshToken.Equals(user.RefreshToken.Token) && !user.RefreshToken.IsExpired)
                {
                    Debug.WriteLine($"User {response.Username} refreshing JWT token");
                    return await Authorization(new UserAuthenticationRequest(user.Username, user.Password));
                }
                else
                    return Unauthorized("JWT token expired");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                return NotFound($"Database error: {e}");
            }
        }

        [HttpGet("/user/getUsernames")]
        public async Task<ActionResult<IList<string>>> GetUsernames([FromHeader] string Authorization)
        {
            UserAuthorizationRequest _userHeader;
            try
            {
                _userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
                if (!_tokenHandlerService.ValidateToken(_userHeader.JwtToken))
                    return Unauthorized("Token error");
                var list = await _databaseService.GetUsernamesAsync();
                return Ok(list);
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return BadRequest($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                return NotFound($"Database error: {e}");
            }
        }
    }
}