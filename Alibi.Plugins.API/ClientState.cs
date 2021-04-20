namespace Alibi.Plugins.API
{
    public enum ClientState
    {
        /// <summary>
        /// Brand new client: Has not sent any packets yet
        /// </summary>
        NewClient,
        /// <summary>
        /// Has sent the server a protocol-correct handshake, but has not identified yet.
        /// </summary>
        PostHandshake,
        /// <summary>
        /// Has told the server their client, we are about to send them server features
        /// </summary>
        Identified,
        /// <summary>
        /// The client is fully connected, and has joined an area. Ready to play.
        /// </summary>
        InArea
    }
}