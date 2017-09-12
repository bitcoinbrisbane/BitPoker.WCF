using System;
using System.Collections.Generic;
using System.Text;
using PokerConsole.Engine;
using PokerEngine.Engine;
using PokerEngine;

namespace UltimatePoker.Engine
{
    /// <summary>
    /// An extended interface of the client helpers, this provides additional capabilities to provide GUI responses
    /// </summary>
    public interface IGuiClientHelper : IClientHelper
    {
        /// <summary>
        /// This event is raised when the clinet responds, the client should be ready getting new notifications.
        /// </summary>
        event EventHandler GuiClientResponded;

        /// <summary>
        /// This event is raised when the user aborts the game.
        /// </summary>
        /// <remarks>
        /// The value of the data event args indicates if the client should abort immidiatly
        /// </remarks>
        event DataEventHandler<bool> GameAborted;

        /// <summary>
        /// This event is raised when the user wants to send a chat message
        /// </summary>
        event DataEventHandler<string> UserChats;

        /// <summary>
        /// This method is called when there is a need for a time limit timer to start
        /// </summary>
        /// <param name="timeout">The remaining time of the player action
        /// </param>
        void StartActionTimer(TimeSpan timeout);
        /// <summary>
        /// Notify the Gui Helper of the already connected player
        /// </summary>
        /// <param name="players">A list of players which are currently connected to the server</param>
        void NotifyLoggedinPlayers(IEnumerable<Player> players);

        /// <summary>
        /// Notify the Gui Helper that the server waits for this client to repond to close
        /// the server. Must raise <see cref="GameAborted"/> to signal the server to close.
        /// </summary>
        void NotifyServerWaitsToClose();
    }
}
