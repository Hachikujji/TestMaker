using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using TestMaker.Database.Models;
using TestMaker.Database.Services;
using TestMakerApi.Properties;

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

        [HttpGet("/getUsers")]
        public IEnumerable<UsersDTO> GetUsers()
        {
            var mapper = new AutoMapper.MapperConfiguration(cfg => cfg.CreateMap<Users, UsersDTO>()).CreateMapper();

            var l = _databaseService.GetUsers().Result;
            return mapper.Map<IEnumerable<Users>, List<UsersDTO>>(l);
        }

        [HttpPost("/addUser")]
        public ActionResult<Users> AddUsers(Users user)
        {
            _databaseService.AddUser(user);

            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPost("/token")]
        public IActionResult Token(string username, string password)
        {
            var user = _databaseService.GetUser(username, password).Result;
            if (user == null)
                return BadRequest(new { errorText = "Invalid username or password." });

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                username = user.Username
            };

            return Json(response);
        }

        [HttpGet("/getUser/{id}")]
        public ActionResult<Users> GetUser(int id)
        {
            var user = _databaseService.GetUser(id).Result;

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }
    }
}