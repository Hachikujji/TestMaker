using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        //[HttpGet("/user/getUsers")]
        //public IEnumerable<UsersDTO> GetUsers()
        //{
        //    var mapper = new AutoMapper.MapperConfiguration(cfg => cfg.CreateMap<User, UsersDTO>()).CreateMapper();

        //    var l = _databaseService.GetUsersAsync().Result;
        //    return mapper.Map<IEnumerable<User>, List<UsersDTO>>(l);
        //}

        [HttpPost("/user/addUser")]
        public ActionResult<UserAuthenticationRequest> AddUsers(UserAuthenticationRequest userInfo)
        {
            var user = new User(userInfo.Username, userInfo.Password);
            _databaseService.AddUserAsync(user);

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        [AllowAnonymous]
        [HttpPost("/user/authorization")]
        public ActionResult<UserAuthenticationResponse> Authorization(UserAuthenticationRequest model)
        {
            var user = _databaseService.GetUserAsync(model.Username, model.Password).Result;
            if (user == null)
                return BadRequest(new { errorText = "Invalid username or password." });

            var jwtToken = _tokenHandlerService.CreateJwtToken();
            var refreshToken = _tokenHandlerService.CreateRefreshToken();

            user.RefreshToken = refreshToken;
            _databaseService.UpdateUserAsync(user);

            return Ok(new UserAuthenticationResponse(user, jwtToken, refreshToken.Token));
        }

        [HttpGet("/user/getUser/{id}")]
        public ActionResult<User> GetUserById(int id, [FromHeader] string Authorization)
        {
            UserAuthorizationRequest userHeader = JsonConvert.DeserializeObject<UserAuthorizationRequest>(Authorization);
            if (!_tokenHandlerService.ValidateToken(userHeader.JwtToken))
                return Unauthorized("Token error");

            var user = _databaseService.GetUserAsync(id).Result;

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpGet("/user/isUserExists/{username}")]
        public ActionResult<User> IsUserExists(string username)
        {
            var userExists = _databaseService.IsUserExistsAsync(username).Result;
            if (userExists == false)
                return NotFound();

            return Ok(userExists);
        }
    }
}