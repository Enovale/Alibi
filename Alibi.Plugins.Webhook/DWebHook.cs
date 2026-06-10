using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Alibi.Plugins.Webhook
{
    public class DWebHook : IDisposable
    {
        public string WebHook { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }

        private static readonly Dictionary<string, string> DiscordValues = new();

        private readonly HttpClient _dWebClient;

        public DWebHook(string url = "")
        {
            WebHook = url;
            _dWebClient = new HttpClient();
        }

        public async Task SendMessage(string msgSend)
        {
            if (string.IsNullOrWhiteSpace(WebHook))
                return;
            
            DiscordValues.Clear();
            DiscordValues.Add("username", Username);
            DiscordValues.Add("avatar_url", AvatarUrl);
            DiscordValues.Add("content", msgSend);

            using var postContent = new FormUrlEncodedContent(DiscordValues);
            using var response = await _dWebClient.PostAsync(WebHook, postContent);
            response.EnsureSuccessStatusCode(); // Throw if httpcode is an error
        }

        public void Dispose()
        {
            _dWebClient.Dispose();
        }
    }
}