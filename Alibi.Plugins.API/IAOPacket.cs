namespace Alibi.Plugins.API
{
    public interface IAOPacket
    {
        /// <summary>
        /// The type (ID) of this packet. E.g HI, RT, CT
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Data contained in this packet, that will be joined together when sent.
        /// </summary>
        public string[] Objects { get; set; }
    }
}