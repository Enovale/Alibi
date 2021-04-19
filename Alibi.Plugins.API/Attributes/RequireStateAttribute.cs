using System;

namespace Alibi.Plugins.API.Attributes
{
    /// <summary>
    /// Specifies that a message handler requires the client to be in a specific state in order to process.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireStateAttribute : Attribute
    {
        public ClientState State { get; }
        public bool Kick { get; }

        /// <param name="requiredState">The state the client must be in for the message to be handled.</param>
        /// <param name="kickIfFalse">Should we kick the player if the state is mismatched?</param>
        public RequireStateAttribute(ClientState requiredState, bool kickIfFalse = true)
        {
            State = requiredState;
            Kick = kickIfFalse;
        }
    }
}