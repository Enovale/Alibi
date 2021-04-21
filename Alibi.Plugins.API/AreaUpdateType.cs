namespace Alibi.Plugins.API
{
    /// <summary>
    /// Specifies which information to update about an area.
    /// </summary>
    public enum AreaUpdateType
    {
        PlayerCount = 0,
        /// <summary>
        /// Update the status string for this area (determines area color, and joining rules)
        /// </summary>
        Status = 1,
        CourtManager = 2,
        Locked = 3
    }
}