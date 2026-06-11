using System;

namespace Alibi.Plugins.Cerberus
{
    [Serializable]
    public class CerberusConfiguration
    {
        /// <summary>
        /// How many diacritics are allowed for one character in a message.
        /// <remarks>
        /// Excessive diacritic spam is often referred to as "zalgo",
        /// this is to prevent that from impacting readability or breaking clients.
        /// </remarks>
        /// </summary>
        public int DiacriticLimit { get; set; } = 5;
        
        public bool AllowOocDoublePosting { get; set; }
        public int MaxIcMessagesPerSecond { get; set; } = 2;
        public int IcMuteLengthInSeconds { get; set; } = 5;
        public int MaxOocMessagesPerSecond { get; set; } = 2;
        public int OocMuteLengthInSeconds { get; set; } = 5;
        public int MaxMusicMessagesPerSecond { get; set; } = 2;
        public int MusicMuteLengthInSeconds { get; set; } = 5;
    }
}