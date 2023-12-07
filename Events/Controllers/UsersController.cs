using Events.Configure;
using Events.Data;
using Events.Dto;
using Events.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Events.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;
        public UsersController(ApplicationDbContext context, IConfiguration config)
        {
            _config = config;
            _context = context;
        }
        private int GetUserId()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            return int.Parse(userId!);
        }
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserRegisterDto userDto)
        {
            Boolean hasNull = false;
            string theNull = "";
            foreach (PropertyInfo property in userDto.GetType().GetProperties())
            {
                object value = property.GetValue(userDto)!;
                if (value == null)
                {
                    hasNull = true;
                    theNull = $"Property '{property.Name}' is null.";
                }
            }
            if (hasNull)
            {
                return BadRequest(new
                {
                    success = false,
                    message = theNull
                });
            };
            var currentUser = _context.Users.FirstOrDefault(x => x.Email!.ToLower() == userDto.Email!.ToLower());

            if (currentUser is null)
            {
                User user = new()
                {
                    Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    PhoneNumber = userDto.PhoneNumber,
                    Email = userDto.Email
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    success = true,
                    data = user.ToJson()
                });
            }
            else
            {
                return Conflict(new
                {
                    success = false,
                    message = "the user is exist"
                });
            }
        }
        [HttpPost("LogIn")]
        [AllowAnonymous]
        public ActionResult LogIn(UserDto user)
        {
            var currentUser = _context.Users.FirstOrDefault(x => x.Email!.ToLower() == user.Email!.ToLower());
            if (currentUser == null || !BCrypt.Net.BCrypt.Verify(user.Password, currentUser.Password))
            {
                return Unauthorized(new { success = false, message = "wrong email or password" });
            }
            
            GenerateToken g = new GenerateToken(_context, _config);
            string theToken = g.GenerateApiToken(currentUser.Id);
            
            
            return Ok(new
            {
                success = true,
                message = "success",
                data = new
                {
                    token = theToken,
                    user = currentUser.ToJson()
                }
            });


        }
    }
}
