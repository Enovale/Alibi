namespace AO2Sharp.Plugins.Webhook
{
    internal class WebhookConfig
    {
        public string WebhookUrl { get; set; }
        public string Username { get; set; }
        public string AvatarURL { get; set; }
        public string Message { get; set; } = "Someone called for moderator in %a: \"%r\"";
    }
}
