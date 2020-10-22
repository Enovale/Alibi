using System;

namespace Alibi.Plugins.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AdminOnlyAttribute : Attribute
    {
        public string ErrorMsg { get; }

        public AdminOnlyAttribute(string errorMsg = "You are not an admin, and can't run this command.")
        {
            ErrorMsg = errorMsg;
        }
    }
}