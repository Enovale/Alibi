using System;

namespace Alibi.Plugins.Cerberus
{
    public class MuteInfo
    {
        public int IcMessages { get; set; } = 0;
        public bool IcMuted { get; set; } = false;
        public DateTime IcTimer { get; set; } = DateTime.UtcNow;
        public int MusicMessages { get; set; } = 0;
        public bool MusicMuted { get; set; } = false;
        public DateTime MusicTimer { get; set; } = DateTime.UtcNow;
        public int OocMessages { get; set; } = 0;
        public bool OocMuted { get; set; } = false;
        public DateTime OocTimer { get; set; } = DateTime.UtcNow;
    }
}