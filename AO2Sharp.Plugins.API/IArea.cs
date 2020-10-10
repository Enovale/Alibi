using System;
using System.Collections.Generic;
using System.Text;

namespace AO2Sharp.Plugins.API
{
    public interface IArea
    {
        public string Name { get; set; }
        public string Background { get; set; }
        public int BackgroundPosition { get; set; }
        public bool Locked { get; set; }
        public bool BackgroundLocked { get; set; }
        public bool IniSwappingAllowed { get; set; }
    }
}
