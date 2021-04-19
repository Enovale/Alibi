using System;

namespace Alibi.Plugins.API.Attributes
{
    /// <summary>
    /// Specifies a command that can only be run by moderators.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ModOnlyAttribute : Attribute
    {
        public string ErrorMsg { get; }

        /// <param name="errorMsg">The error message to send in the OOC if the user is not a moderator.</param>
        public ModOnlyAttribute(string errorMsg = "You are not moderator, and can't run this command.")
        {
            ErrorMsg = errorMsg;
        }
    }
}