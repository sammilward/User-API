using System;

namespace UserAPI.Contracts.V1.Requests
{
    public class UpdateUserRequest
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BirthCountry { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime? DOB { get; set; }
        public bool? Male { get; set; }
    }
}
