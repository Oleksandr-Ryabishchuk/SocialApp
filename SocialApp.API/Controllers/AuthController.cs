using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SocialApp.API.DTOs;
using SocialApp.API.InterfaceRepositories;
using SocialApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace SocialApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        public AuthController(IAuthRepository authRepository, IConfiguration configuration)
        {
            _configuration = configuration;
            _authRepository = authRepository;

        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            // validate the request, take data from db table user
            // Use in mvc controller
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            userForRegisterDto.UserName = userForRegisterDto.UserName.ToLower();
            // make sure the names will save in lower case

            if (await _authRepository.UserExists(userForRegisterDto.UserName))
            {
                return BadRequest("Username already registered");
            }

            var userToCreate = new User
            {
                UserName = userForRegisterDto.UserName
            };

            var createdUser = await _authRepository
            .Register(userToCreate, userForRegisterDto.Password);

            return StatusCode(201);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            // throw new Exception("Computer doesn't run command");
            
            var userFromRepository = await _authRepository.Login(userForLoginDto.UserName.ToLower(),
             userForLoginDto.Password);

            if (userFromRepository == null)
            {
                return Unauthorized();
            }

            var claims = new[]
            {
                 new Claim(ClaimTypes.NameIdentifier, userFromRepository.UserId.ToString()),
                 new Claim(ClaimTypes.Name, userFromRepository.UserName)
             };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            
            var signInCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
           
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(2),
                SigningCredentials = signInCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });

            
        }
    }
}