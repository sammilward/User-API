namespace UserAPI.RabbitMQ.Requests
{
    public class GetAllUsersRabbitRequest
    {
        public string Id { get; set; }
        public string LocationScope { get; set; } = "City";
        public bool Travellers { get; set; } = true;
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public int GenderOption { get; set; }
    }
}
