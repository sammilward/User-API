using Microsoft.AspNetCore.Identity;
using UserAPI.Models;

namespace UserAPI.RabbitMQ.Responses
{
    public class CreateUserRabbitResponse
    {
        public IdentityResult IdentityResult { get; set; }
        public User User { get; set; }
    }
}
