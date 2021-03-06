using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TestMaker.Database.Entities;
using TestMaker.Database.Models;
using TestMakerApi.Services;

namespace TestMakerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthorizationController : Controller
    {
        #region Private Fields

        private readonly IAuthorizationControllerService _autorizationControllerService;
        private readonly ITokenHandlerService _tokenHandlerService;

        #endregion Private Fields

        #region Public Constructors

        public AuthorizationController(IAuthorizationControllerService authorizationControllerService, ITokenHandlerService tokenHandlerService)
        {
            _autorizationControllerService = authorizationControllerService;
            _tokenHandlerService = tokenHandlerService;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Add user
        /// </summary>
        /// <param name="userInfo">Username and password</param>
        [HttpPost("/user/addUser")]
        public async Task<ActionResult<User>> AddUsers(UserAuthenticationRequest userInfo)
        {
            var user = new User(userInfo.Username, userInfo.Password);
            try
            {
                if (!string.IsNullOrWhiteSpace(userInfo.Username) && !string.IsNullOrWhiteSpace(userInfo.Password) && !(await _autorizationControllerService.IsUserExistsAsync(userInfo.Username)))
                {
                    await _autorizationControllerService.AddUserAsync(user);
                    return Ok(user);
                }
                else
                    return BadRequest("Invalid username or password.");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Authorization of user. Creates tokens and send them to user
        /// </summary>
        /// <param name="model">username and password</param>
        [HttpPost("/user/authorization")]
        public async Task<ActionResult<UserAuthenticationResponse>> Authorization(UserAuthenticationRequest model)
        {
            User user;
            try
            {
                user = await _autorizationControllerService.GetUserAsync(model.Username, model.Password);
                if (user == null)
                    return BadRequest("Invalid username or password.");
                var jwtToken = _tokenHandlerService.CreateJwtToken(user.Id, user.Username);
                var refreshToken = _tokenHandlerService.CreateRefreshToken();
                await _autorizationControllerService.UpdateUserRefreshTokenAsync(user, refreshToken);
                return Ok(new UserAuthenticationResponse(user, jwtToken, refreshToken.Token));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Is user exists
        /// </summary>
        /// <param name="username">Username</param>
        [HttpGet("/user/isUserExists/{username}")]
        public async Task<ActionResult<bool>> IsUserExists(string username)
        {
            try
            {
                var isUserExists = await _autorizationControllerService.IsUserExistsAsync(username);
                return Ok(isUserExists);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Refresh JWT token of user
        /// </summary>
        /// <param name="response"></param>
        [HttpPost("/user/refreshToken")]
        public async Task<ActionResult<UserAuthenticationResponse>> RefreshToken(UserAuthenticationResponse response)
        {
            try
            {
                var user = await _autorizationControllerService.GetUserByIdAsync(response.Id);
                if (response.RefreshToken.Equals(user.RefreshToken.Token) && !user.RefreshToken.IsExpired)
                {
                    return await Authorization(new UserAuthenticationRequest(user.Username, user.Password));
                }
                else
                    return Unauthorized("JWT token expired");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get string list of usernames
        /// </summary>
        [HttpGet("/user/getUsernames")]
        [Authorize]
        public async Task<ActionResult<IList<string>>> GetUsernames()
        {
            try
            {
                var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                var list = await _autorizationControllerService.GetUsernamesAsync();
                return Ok(list);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion Public Methods
    }
}