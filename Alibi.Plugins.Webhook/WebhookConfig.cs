// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Alibi.Plugins.Webhook
{
    public class WebhookConfig
    {
        public string WebhookUrl { get; set; }
        public string Username { get; set; }
        public string AvatarURL { get; set; }
        public string ModMessage { get; set; } = "Someone called for moderator in %a: \"%r\"";
        public string BanMessage { get; set; } = "A player was banned: %r";
    }
}
