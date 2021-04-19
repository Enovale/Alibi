using System;

namespace Alibi.Plugins.API.Attributes
{
    /// <summary>
    /// Makes sure a command can oly be run as an admin user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AdminOnlyAttribute : Attribute
    {
        public string ErrorMsg { get; }

        /// <param name="errorMsg">The error message to send in the OOC if the user is not admin.</param>
        public AdminOnlyAttribute(string errorMsg = "You are not an admin, and can't run this command.")
        {
            ErrorMsg = errorMsg;
        }
    }
}