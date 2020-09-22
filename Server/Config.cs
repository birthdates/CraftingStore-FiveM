using Newtonsoft.Json;

namespace Server
{
    public class Config
    {
        [JsonProperty("API Token Key")] public string ApiKey = "ENTER_KEY_HERE";

        [JsonProperty("Refresh Time (seconds, has to above 5 minutes)")]
        public int RefreshTime = 300;
    }
}