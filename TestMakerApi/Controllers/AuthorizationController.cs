using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using TestMaker.Database.Entities;
using TestMaker.Database.Models;
using TestMaker.Database.Services;

namespace TestMakerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthorizationController : Controller
    {
        private IDatabaseService _databaseService;

        public AuthorizationController(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        //[HttpGet("/user/getUsers")]
        //public IEnumerable<UsersDTO> GetUsers()
        //{
        //    var mapper = new AutoMapper.MapperConfiguration(cfg => cfg.CreateMap<User, UsersDTO>()).CreateMapper();

        //    var l = _databaseService.GetUsersAsync().Result;
        //    return mapper.Map<IEnumerable<User>, List<UsersDTO>>(l);
        //}

        [HttpPost("/user/addUser")]
        public ActionResult<UserAuthorizationRequest> AddUsers(UserAuthorizationRequest userInfo)
        {
            var user = new User(userInfo.Username,userInfo.Password);
            _databaseService.AddUserAsync(user);

            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [AllowAnonymous]
        [HttpPost("/user/authorization")]
        public ActionResult<UserAuthorizationRequest> Authorization(UserAuthorizationRequest model)
        {
            var user = _databaseService.GetUserAsync(model.Username, model.Password).Result;
            if (user == null)
                return BadRequest(new { errorText = "Invalid username or password." });

            var jwtToken = _databaseService.CreateJwtToken(user);
            var refreshToken = _databaseService.CreateRefreshToken();

            user.RefreshToken = refreshToken;
            _databaseService.UpdateUserAsync(user);

            // cookies
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

            return Ok(new UserAuthorizationResponse(user, jwtToken, refreshToken.Token));
        }

        [HttpGet("/user/getUser/{id}")]
        public ActionResult<User> GetUser(int id)
        {
            var user = _databaseService.GetUserAsync(id).Result;

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }
    }
}