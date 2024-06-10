using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Models;
using WebAPI.Services;
using WebAPI.DTOs;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public UserController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginRequestDto request)
        {
            var user = await _userService.LoginAsync(request.Username, request.Password);

            if (user == null)
            {
                return Unauthorized();
            }

            var token = GenerateJwtToken(user);
            return Ok(token);
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterRequestDto request)
        {
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                CustomRole = request.Role
            };

            try
            {
                await _userService.RegisterAsync(user);
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim("CustomRole", user.CustomRole.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}