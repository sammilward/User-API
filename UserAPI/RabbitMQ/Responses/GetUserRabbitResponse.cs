﻿using UserAPI.Models;

namespace UserAPI.RabbitMQ.Responses
{
    public class GetUserRabbitResponse
    {
        public bool FoundUser { get; set; } = true;
        public User User { get; set; }
    }
}
