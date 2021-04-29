using System;

namespace Alibi.Plugins.API
{
    /// <summary>
    /// Flags representing a client's casing preferences (See SETCASE# and CASEA#)
    /// </summary>
    [Flags]
    public enum CasingFlags
    {
        None = 0,
        CaseManager = 1,
        Defense = 2,
        Prosecutor = 4,
        Judge = 8,
        Jury = 16,
        Stenographer = 32
    }
}