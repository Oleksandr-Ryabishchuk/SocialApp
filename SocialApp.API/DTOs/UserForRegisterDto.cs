using System.ComponentModel.DataAnnotations;

namespace SocialApp.API.DTOs
{
    public class UserForRegisterDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 4, 
        ErrorMessage = "You must specify password between 4 and 8 characters")]
        public string Password { get; set; }
    }
}