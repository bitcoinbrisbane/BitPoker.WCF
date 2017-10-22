using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerEngine;
using PokerEngine.Engine;

namespace PokerService
{
    /// <summary>
    /// This interface defines a helping class which can be composed inside a <see cref="BaseFiveCardEngine"/> and 
    /// provide common user input/output operations.
    /// </summary>
    public interface IFiveCardEngineHelper : IEngineHelper
    {
        /// <summary>
        /// A method which is used to get the player drawing action. Derived classes must override this method to perform the action.
        /// </summary>
        /// <param name="player">The player which may draw new cards</param>
        /// <param name="action">The drawing action which must be updated with the player drawing selection</param>
        void WaitPlayerDrawingAction(Player player, PlayerDrawingAction action);

        /// <summary>
        /// Called by the engine to notify the player with the new cards
        /// </summary>
        /// <param name="curPlayer">The player with the new drawn cards</param>
        void NotifyPlayerNewCards(Player curPlayer);

        /// <summary>
        /// Called by the engine after the drawing round completes.
        /// </summary>
        void NotifyDrawComplete();
    }
}
