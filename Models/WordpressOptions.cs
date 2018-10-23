using Newtonsoft.Json;

namespace DuploAuth.Models
{
    public class WordpressOptions
    {
        public class WordpressAuthSettings
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }

        public class WordpressAuthRequest
        {
            [JsonProperty("code")]
            public string AccessToken { get; set; }

            [JsonProperty("redirect_uri")]
            public string RedirectURI { get; set; }
        }

        internal class WordpressUserAccessTokenData
        {
            [JsonProperty("ID")]
            public string Id { get; set; }

            [JsonProperty("user_nicename")]
            public string NiceName { get; set; }

            [JsonProperty("display_name")]
            public string Name { get; set; }

            [JsonProperty("user_status")]
            public int Status { get; set; }

            [JsonProperty("user_login")]
            public string UserId { get; set; }

            [JsonProperty("user_email")]
            public string Email { get; set; }
        }

        internal class WordpressAppAccessToken
        {
            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("access_token")]
            public string AccessToken { get; set; }
        }
    }
}