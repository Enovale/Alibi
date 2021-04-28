namespace Alibi.Plugins.API
{
    /// <summary>
    /// Represents the underlying TCP/Websocket session for a client.
    /// </summary>
    /// <remarks>
    /// Warning! Here be dragons. This is interfacing with a dependency of the project so even I don't
    /// fully understand it or have control over how it works.
    /// </remarks>
    public interface ISession
    {
        /// <summary>
        /// Forcefully disconnects a client session
        /// </summary>
        /// <returns>True if the session was disconnected, false if the session was already disconnected.</returns>
        public bool Disconnect();

        /// <summary>
        /// Send a raw text packet to this session
        /// </summary>
        /// <param name="text">The text to send</param>
        /// <returns>The sending byte buffer, I guess TODO: Better understand what this returns</returns>
        public long Send(string text);

        /// <summary>
        /// Send a raw array of bytes to this session
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public long Send(byte[] buffer, long length, long size);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool SendAsync(string text);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool SendAsync(byte[] buffer, long length, long size);
    }
}