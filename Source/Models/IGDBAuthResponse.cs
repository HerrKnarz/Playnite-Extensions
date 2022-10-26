using Newtonsoft.Json;

namespace LinkUtilities.Models
{
    /// <summary>
    /// Response after authentication at IGDB
    /// </summary>
    public class IGDBAuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken;

        [JsonProperty("expires_in")]
        public int ExpiresIn;

        [JsonProperty("token_type")]
        public string TokenType;
    }
}
