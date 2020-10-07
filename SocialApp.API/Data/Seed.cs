using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using SocialApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialApp.API.Data
{
    public class Seed
    {
        public static void SeedUsers(UserManager<User> manager, RoleManager<Role> roleManager)
        {
            if (!manager.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                var roles = new List<Role> 
                {
                    new Role{Name = "Member"},
                    new Role{Name = "Admin"},
                    new Role{Name = "Moderator"},
                    new Role{Name = "VIP"}
                };

                foreach(var role in roles) 
                {
                    roleManager.CreateAsync(role).Wait();
                } 

                foreach (var user in users)
                {
                    manager.CreateAsync(user, "password").Wait();
                    manager.AddToRoleAsync(user, "Member");
                }

                var adminUser = new User {
                    UserName = "Admin"
                };

                var result = manager.CreateAsync(adminUser, "password").Result;

                if(result.Succeeded) 
                {
                    var admin = manager.FindByNameAsync("Admin").Result;
                    manager.AddToRolesAsync(admin, new [] {"Admin", "Moderator"});
                }
               
            }
        }
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
