using System.Threading.Tasks;
using SocialApp.API.Models;

namespace SocialApp.API.InterfaceRepositories
{
    public interface IAuthRepository
    {
           Task<User>  Register(User user, string password);
           Task<User> Login(string userName, string password);
           Task<bool> UserExists(string userName);
          
    }
}