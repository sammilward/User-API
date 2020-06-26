namespace UserAPI.Contracts.V1.Routes
{
    public class Routes
    {
        public const string Version = "v1";

        public const string Base = "/" + Version;

        public static class UserRoutes
        {
            public const string GetAll = Base + "/users";

            public const string Get = Base + "/user/{id}";

            public const string Create = Base + "/user";

            public const string Delete = Base + "/user";

            public const string Update = Base + "/user";
        }
    }
}
