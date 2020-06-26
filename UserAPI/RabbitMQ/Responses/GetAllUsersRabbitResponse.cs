using System.Collections.Generic;
using UserAPI.Models;

namespace UserAPI.RabbitMQ.Responses
{
    public class GetAllUsersRabbitResponse
    {
        public bool FoundUsers { get; set; }
        public List<User> Users { get; set; }
    }
}
