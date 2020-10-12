using System;
using System.Collections.Generic;
using System.Text;

namespace AO2Sharp.Plugins.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ModOnlyAttribute : Attribute
    {
        public string ErrorMsg { get; }

        public ModOnlyAttribute(string errorMsg = "You are not moderator, and can't run this command.")
        {
            ErrorMsg = errorMsg;
        }
    }
}
