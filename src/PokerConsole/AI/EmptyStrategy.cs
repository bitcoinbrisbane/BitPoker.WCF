using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerEngine;
using PokerEngine.Engine;

namespace PokerConsole.AI
{
    /// <summary>
    /// An empty AI strategy which does nothing
    /// </summary>
    public class EmptyStrategy : AIStrategy
    {
        /// <summary>
        /// Called by the client when an update arrives. Does nothing
        /// </summary>
        /// <param name="syhcronizationData">The players updated. Must not be null</param>
        public override void Synchronize(IEnumerable<Player> syhcronizationData)
        {

        }
        /// <summary>
        /// Called by the client when a bet decision should be made. Does nothing to the action
        /// </summary>
        /// <param name="player">The automated player. 
        /// </param>
        /// <param name="action">The betting action which must be modified to pass the client response</param>
        public override void Bet(Player player, PlayerBettingAction action)
        {

        }

        /// <summary>
        /// Called by a client which needs to manually draw cards. Draws no cards
        /// </summary>
        /// <param name="player">The automated player</param>
        /// <param name="action">The drawing action which must be modified to pass the strategy decision</param>
        public override void Draw(Player player, PlayerDrawingAction action)
        {

        }
    }
}
