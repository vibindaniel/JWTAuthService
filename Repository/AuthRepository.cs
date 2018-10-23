using System;

namespace DuploAuth.Repository
{
    public class AuthRepository
    {
        public static bool ValidateProviders(string provider)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable($"{provider.ToUpper()}_CLIENTID")) ||
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable($"{provider.ToUpper()}_CLIENT_SECRET")))
            {
                return false;
            }
            return true;
        }

        public static string getAuthorizationEndpoint(string provider)
        {
            if (provider == "wordpress")
            {
                return $"{Environment.GetEnvironmentVariable("WORDPRESS_ENDPOINT")}/oauth/authorize?response_type=code&client_id={Environment.GetEnvironmentVariable($"WORDPRESS_CLIENTID")}&redirect_uri=";
            }
            return string.Empty;
        }
    }
}