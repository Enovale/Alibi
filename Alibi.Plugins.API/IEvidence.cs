namespace Alibi.Plugins.API
{
    /// <summary>
    /// Represents a piece of evidence in the server.
    /// </summary>
    public interface IEvidence
    {
        /// <summary>
        /// The name of this piece of evidence.
        /// </summary>
        /// <remarks>
        /// This is completely arbitrary, and determined by user input. Be careful out there.
        /// </remarks>
        public string Name { get; set; }
        /// <summary>
        /// The description for this piece of evidence.
        /// </summary>
        /// <remarks>
        /// This is completely arbitrary, and determined by user input. Be careful out there.
        /// </remarks>
        public string Description { get; set; }
        /// <summary>
        /// The photo name of this piece of evidence.
        /// </summary>
        /// <remarks>
        /// This is completely arbitrary, and determined by user input. Be careful out there.
        /// </remarks>
        public string Photo { get; set; }

        /// <summary>
        /// Converts this piece of evidence into a packet that can be sent to players
        /// </summary>
        /// <returns>A packet version of this piece of evidence, to be used as data in a packet.</returns>
        /// <code>
        /// client.Send(new AOPacket("EI", index.ToString(), client.Area.EvidenceList[index].ToPacket()));
        /// </code>
        public string ToPacket();
    }
}