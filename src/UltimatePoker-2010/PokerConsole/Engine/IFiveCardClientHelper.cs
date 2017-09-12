using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerEngine;
using PokerEngine.Engine;

namespace PokerConsole.Engine
{
    /// <summary>
    /// An interface which describes five card client helpers
    /// </summary>
    public interface IFiveCardClientHelper : IClientHelper
    {
        /// <summary>
        /// Called by the client when a drawing round starts
        /// </summary>
        void NotifyDrawingRoundStarted();
        /// <summary>
        /// Called by the client when a drawing round completes
        /// </summary>
        void NotifyDrawingRoundCompleted();

        /// <summary>
        /// Called by the client to notify how many cards were drawn by the player
        /// </summary>
        /// <param name="player">The drawing players</param>
        /// <param name="drawCount">The amount of cards drawn. Can be 0</param>
        void NotifyPlayerDraws(Player player, int drawCount);
        /// <summary>
        /// Updates the current client with the new cards drawn.
        /// </summary>
        /// <param name="player">
        /// The player with the new updated cards
        /// </param>
        void NotifyPlayerNewCards(Player player);
        /// <summary>
        /// Called by the client to get the player drawing response.
        /// </summary>
        /// <param name="player">The player which draws the cards</param>
        /// <param name="action">The action which should be modified according to the player actions</param>
        void WaitPlayerDrawingAction(Player player, PlayerDrawingAction action);
    }
}
