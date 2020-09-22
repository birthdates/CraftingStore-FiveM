using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace Server
{
    public class CraftingStore : BaseScript
    {
        private const string BaseApiUrl = "https://api.craftingstore.net/v4/";
        private readonly Dictionary<int, Action<string>> _httpCallbacks = new Dictionary<int, Action<string>>();
        private Config _config;

        public CraftingStore()
        {
            EventHandlers["__cfx_internal:httpResponse"] += new Action<int, int, string, dynamic>(HttpListener);
            LoadConfig();
            if (_config.RefreshTime < 300)
            {
                _config.RefreshTime = 300;
                SaveConfig();
                Log("The refresh time in the config was below 5 minutes.");
            }

            Tick += CheckTick;
        }

        private async Task CheckTick()
        {
            HttpRequest($"{BaseApiUrl}queue", "GET", string.Empty, response =>
            {
                var parsedResponse = ParseResponse(response);
                if (!parsedResponse.success)
                {
                    Log("Invalid response returned, please contact us if this error persists.");
                    return;
                }

                var donations = parsedResponse.result;

                var ids = new List<int>();

                foreach (var donation in donations)
                {
                    // Add donation to executed list.
                    ids.Add(donation.id);
                    if (string.IsNullOrEmpty(donation.command)) continue;
                    // Execute commands
                    var fullArgs = donation.command.Split(' ').ToList();
                    var serverEvent = fullArgs[0];
                    Log("Executing the event " + donation.command);
                    TriggerEvent(serverEvent, fullArgs.Where(arg => fullArgs.IndexOf(arg) != 0).ToArray<object>());
                }

                if (ids.Count <= 0) return;
                // Mark as complete if there are commands processed
                var serializedIds = JsonConvert.SerializeObject(ids);

                var payload = "removeIds=" + serializedIds;
                HttpRequest($"{BaseApiUrl}queue/markComplete", "POST", payload);
            });
            await Delay(_config.RefreshTime * 1000);
        }

        #region Helpers

        private static void Log(string message)
        {
            Debug.WriteLine($"[CraftingStore] {message}");
        }

        private void HttpListener(int token, int status, string text, dynamic header)
        {
            if (!_httpCallbacks.TryGetValue(token, out var cb)) return;
            if (status != 200) Debug.WriteLine($"{token} sent back code {status}");
            cb.Invoke(text);
            _httpCallbacks.Remove(token);
        }

        private void HttpRequest(string webUrl, string webMethod, object webData, Action<string> cb = null)
        {
            var request = JsonConvert.SerializeObject(new
            {
                url = webUrl, data = webData,
                method = webMethod, headers = new Dictionary<string, string> {{"token", _config.ApiKey}}
            });
            var token = API.PerformHttpRequestInternal(request, request.Length);
            if (cb == null) return;
            _httpCallbacks[token] = cb;
        }

        private void LoadConfig()
        {
            var jsonData = API.LoadResourceFile(API.GetCurrentResourceName(), "config.json");
            var defaultConfig = new Config();
            if (jsonData == null)
            {
                _config = defaultConfig;
                SaveConfig();
                return;
            }

            _config = JsonConvert.DeserializeObject<Config>(jsonData);
            var save = false;
            foreach (var field in _config.GetType().GetFields().Where(field => field.GetValue(_config) == null))
            {
                field.SetValue(_config, field.GetValue(defaultConfig));
                save = true;
            }

            if (!save) return;
            SaveConfig();
        }

        private void SaveConfig()
        {
            var jsonData = JsonConvert.SerializeObject(_config, Formatting.Indented);
            API.SaveResourceFile(API.GetCurrentResourceName(), "config.json", jsonData, -1);
        }

        private static ApiResponse ParseResponse(string response)
        {
            var commands = JsonConvert.DeserializeObject<ApiResponse>(response);
            return commands;
        }

        #endregion

        #region Classes

        private struct QueueResponse
        {
            public int id;
            public string command;
            public string packageName;
        }


        private struct ApiResponse
        {
            public int id;
            public bool success;
            public string error;
            public string message;
            public QueueResponse[] result;
        }

        private class Config
        {
            [JsonProperty("API Token Key")] public string ApiKey = "ENTER_KEY_HERE";

            [JsonProperty("Refresh Time (seconds, has to above 5 minutes)")]
            public int RefreshTime = 300;
        }
        
        #endregion
    }
}