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
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace SocialApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(IConfiguration configuration, IMapper mapper,
        UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
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
           
            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);

            if(result.Succeeded)
            {
                var userToReturn = _mapper.Map<UserForDetailedDto>(userToCreate);
                return CreatedAtRoute("GetUser", 
                new {controller = "Users", id = userToCreate.Id}, userToReturn);
            }

            return BadRequest(result.Errors);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            // throw new Exception("Computer doesn't run command");

            var user = await _userManager.FindByNameAsync(userForLoginDto.UserName);

            var result = await _signInManager.CheckPasswordSignInAsync(user, 
            userForLoginDto.Password, false);

            if (result.Succeeded)
            {
               var userToReturn = _mapper.Map<UserForListDto>(user);

                return Ok(new
                {
                    token = GenerateJwtToken(user).Result,
                    user = userToReturn
                });
            }

           return Unauthorized();            
        }

        private async Task<string> GenerateJwtToken(User user) 
        {               
             var claims = new List<Claim>            
             {
                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                 new Claim(ClaimTypes.Name, user.UserName)
             };

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

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

            return tokenHandler.WriteToken(token);
        }
    }
}