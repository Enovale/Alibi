#nullable enable
using System.Collections.Generic;

namespace Alibi.Plugins.API
{
    /// <summary>
    /// An area in the server
    /// </summary>
    public interface IArea
    {
        public string Name { get; }
        
        /// <summary>
        /// Permission level needed to modify evidence
        ///     0 = Free for All
        ///     1 = Case Manager
        ///     2 = No-one
        /// </summary>
        public int EvidenceModifications { get; set; }

        /// <summary>
        /// The background image this area tells clients to use
        /// </summary>
        /// <remarks>
        /// Can be arbitrary depending on server configuration; Try not to hard-code anything.
        /// </remarks>
        public string Background { get; }

        public bool CanLock { get; }
        public bool BackgroundLocked { get; }

        /// <summary>
        /// Whether or not a client can change the character data on their local client. 
        /// This is usually done to test custom characters, or to confuse people.
        /// </summary>
        public bool IniSwappingAllowed { get; }

        /// <summary>
        /// The state of the area, such as LOCKED, CASING, etc.
        /// This is arbitrary, but certain strings will cause clients to give the area different colors.
        /// </summary>
        public string Status { get; set; }

        public string Locked { get; set; }
        
        public int PlayerCount { get; set; }

        /// <summary>
        /// List of Case Managers for this area. Case Managers are allowed to change
        /// more of the area state than most players, in order to prevent spam and 
        /// abuse. Treat these players carefully.
        /// </summary>
        public List<IClient> CurrentCaseManagers { get; }

        /// <summary>
        /// An arbitrary string that should reference some kind of case documentation
        /// for players to refer to. Typically a hyperlink to a website containing a document.
        /// </summary>
        public string? Document { get; set; }

        /// <summary>
        /// Tells clients how much to fill the Defendant HP bar. Doesn't actually do anything.
        /// </summary>
        public int DefendantHp { get; set; }

        /// <summary>
        /// Tells clients how much to fill the Prosecutor HP bar. Doesn't actually do anything.
        /// </summary>
        public int ProsecutorHp { get; set; }

        /// <summary>
        /// List of characters that players in this area are playing as. 
        /// Refer to Server.CharacterList for the order of this list
        /// </summary>
        public bool[] TakenCharacters { get; }

        /// <summary>
        /// List of evidence currently created in this area.
        /// Evidence contains a name, an image, and a description.
        /// </summary>
        public List<IEvidence> EvidenceList { get; }

        /// <summary>
        /// Send a packet to all players currently in this area.
        /// </summary>
        /// <param name="packet">The packet to send</param>
        public void Broadcast(AOPacket packet);

        /// <summary>
        /// Send a message to all players in this area.
        /// The message will display in their Out of Context chat.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="sender">A custom name to display instead of Server:</param>
        public void BroadcastOocMessage(string message, string? sender = null);

        /// <summary>
        /// Broadcast an area update to all clients in this area.
        /// This will only tell them certain information specified by type
        /// </summary>
        /// <param name="type">What kind of update is this?</param>
        public void AreaUpdate(AreaUpdateType type);

        /// <summary>
        /// Send an area update to the specified client.
        /// This will only tell them certain information specified by type
        /// </summary>
        /// <param name="type">What kind of update is this?</param>
        /// <param name="client">The client to which the update will be sent</param>
        public void AreaUpdate(AreaUpdateType type, IClient? client);

        /// <summary>
        /// Broadcast an update of all types to all players.
        /// </summary>
        /// <remarks>
        /// Should be used sparingly as it is needless and expensive.
        /// </remarks>
        public void FullUpdate();

        /// <summary>
        /// Sends an update of all types to this client.
        /// </summary>
        /// <param name="client">Which client to send the updates to</param>
        public void FullUpdate(IClient? client);

        /// <summary>
        /// Returns whether or not this client is a Case Manager.
        /// </summary>
        /// <param name="client">The client to test</param>
        /// <returns>Whether or not this client is a Case Manager</returns>
        public bool IsClientCM(IClient client);

        /// <summary>
        /// Make sure taken characters are updated.
        /// </summary>
        /// <remarks>
        /// This is usually done automatically, but if you manually change
        /// a player's character this will need to be called.
        /// </remarks>
        public void UpdateTakenCharacters();
    }
}