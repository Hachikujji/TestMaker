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
        #region Private Fields
        // all is readonly
        private IDatabaseService _databaseService;
        private ITokenHandlerService _tokenHandlerService;

        #endregion Private Fields

        #region Public Constructors

        public AuthorizationController(IDatabaseService databaseService, ITokenHandlerService tokenHandlerService)
        {
            _databaseService = databaseService;
            _tokenHandlerService = tokenHandlerService;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Add user
        /// </summary>
        /// <param name="userInfo">Username and password</param>
        /// <returns>Ok() if added, BadRequest() if username of password is not valid or already exists, NotFound() if db error</returns>
        [HttpPost("/user/addUser")]
        public async Task<ActionResult<User>> AddUsers(UserAuthenticationRequest userInfo)
        {
            var user = new User(userInfo.Username, userInfo.Password);
            try
            {
                if (!string.IsNullOrWhiteSpace(userInfo.Username) && !string.IsNullOrWhiteSpace(userInfo.Password) && !(await _databaseService.IsUserExistsAsync(userInfo.Username)))
                {
                    await _databaseService.AddUserAsync(user);
                    return Ok(user);
                }
                else
                    return BadRequest("Invalid username or password.");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                return NotFound($"Database error: {e}");
            }
        }

        /// <summary>
        /// Authorization of user. Creates tokens and send them to user
        /// </summary>
        /// <param name="model">username and password</param>
        /// <returns>Ok(UserAuthenticationResponce), BadRequest() if user not found, NotFound() if db error</returns>
        [HttpPost("/user/authorization")]
        public async Task<ActionResult<UserAuthenticationResponse>> Authorization(UserAuthenticationRequest model)
        {
            User user;
            try
            {
                user = await _databaseService.GetUserAsync(model.Username, model.Password);
                if (user == null)
                    return BadRequest("Invalid username or password.");
                var jwtToken = _tokenHandlerService.CreateJwtToken();
                var refreshToken = _tokenHandlerService.CreateRefreshToken();
                await _databaseService.UpdateUserRefreshTokenAsync(user, refreshToken);
                return Ok(new UserAuthenticationResponse(user, jwtToken, refreshToken.Token));
            }
            // ??
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                // ?? why not found
                return NotFound($"Database error: {e}");
            }
        }

        /// <summary>
        /// Validates JWT token
        /// </summary>
        /// <param name="Authorization">Authorization class in header</param>
        /// <returns></returns>
        [HttpGet("/user/validateToken")]
        public ActionResult ValidateToken([FromHeader] string Authorization)//
        {
            if (string.IsNullOrWhiteSpace(Authorization))
                return BadRequest("Token not provided");
            UserAuthorizationRequest userHeader;
            try
            {
                userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
            }
            // ?????
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return BadRequest($"Wrong authorization header: {e}");
            }

            if (userHeader == null || !_tokenHandlerService.ValidateToken(userHeader.JwtToken))
                return Unauthorized("Token is not valid");
            return Ok("Token is valid");
        }
        // what for?
        /// <summary>
        /// Is user exists
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns> Ok(true) if exists, else returns false, NotFound() if db error</returns>
        [HttpGet("/user/isUserExists/{username}")]
        public async Task<ActionResult<bool>> IsUserExists(string username)
        {
            bool isUserExists;
            try
            {
                isUserExists = await _databaseService.IsUserExistsAsync(username);
                return Ok(isUserExists);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
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
            // ??
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                return BadRequest($"Token error: {e}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                return NotFound($"Database error: {e}");
            }
        }

        #endregion Public Methods
    }
}