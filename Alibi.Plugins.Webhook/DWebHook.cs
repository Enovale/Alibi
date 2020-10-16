using System;
using System.Collections.Specialized;
using System.Net;

namespace Alibi.Plugins.Webhook
{

    public class DWebHook : IDisposable
    {
        public string WebHook { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }

        private readonly WebClient _dWebClient;
        private static readonly NameValueCollection DiscordValues = new NameValueCollection();

        public DWebHook(string url = "")
        {
            WebHook = url;
            _dWebClient = new WebClient();
        }

        public void SendMessage(string msgSend)
        {
            if (string.IsNullOrWhiteSpace(WebHook))
                return;
            DiscordValues.Clear();
            DiscordValues.Add("username", Username);
            DiscordValues.Add("avatar_url", AvatarUrl);
            DiscordValues.Add("content", msgSend);

            _dWebClient.UploadValues(WebHook, DiscordValues);
        }

        public void Dispose()
        {
            _dWebClient.Dispose();
        }
    }
}