namespace SocialApp.API.DTOs
{
    public class UserForLoginDto
    {
        public string UserName { get; set; }

        public string Password { get; set; }
        // public byte []  PasswordHash { get; set; }
        // public byte [] PasswordSalt { get; set; }
    }
}