namespace Alibi.Plugins.API
{
    /// <summary>
    /// Contains constants for player authentication levels
    /// </summary>
    public static class AuthType
    {
        /// <summary>
        /// The basic level of player rights. Every new player will have this.
        /// </summary>
        public const int USER = 0;
        /// <summary>
        /// Superior level of player rights. Can do almost anything
        /// except deliberately shut down the server with /stop or /restart
        /// </summary>
        public const int MODERATOR = 1;
        /// <summary>
        /// The highest level of player rights. Can do anything.
        /// </summary>
        public const int ADMINISTRATOR = 2;
    }
}