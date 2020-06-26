namespace UserAPI.Contracts.V1.Requests
{
    public class GetAllUserRequest
    {
        public string LocationScope { get; set; } = "City";
        public bool Travellers { get; set; } = true;
        public int MinAge { get; set; } = 0;
        public int MaxAge { get; set; } = 100;
        public int GenderOption { get; set; } = 2;
        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
    }
}
