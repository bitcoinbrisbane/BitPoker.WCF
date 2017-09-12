using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using PokerEngine;
using PokerRules.Deck;
using PokerRules.Hands;
using PokerRules.Games;
using PokerEngine.Engine;


namespace PokerConsole.Engine
{
    /// <summary>
    /// A base class for five card draw games based on a client helper.
    /// </summary>
    public class FiveGameDrawClient : GameClient<FiveCardDrawGame>, IFiveCardClientHelper
    {
        // the concrete helper to use
        private IFiveCardClientHelper helper;
        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="FiveGameDrawClient"/> class.</para>
        /// </summary>
        /// <param name="helper">The helper to use in this class
        /// </param>
        public FiveGameDrawClient(IFiveCardClientHelper helper)
            : base(helper)
        {
            this.helper = helper;
        }




        /// <summary>
        /// Called by the client when a drawing round starts
        /// </summary>
        public virtual void NotifyDrawingRoundStarted()
        {
            helper.NotifyDrawingRoundStarted();
        }

        /// <summary>
        /// Called by the client when a drawing round completes
        /// </summary>
        public virtual void NotifyDrawingRoundCompleted()
        {
            helper.NotifyDrawingRoundCompleted();
        }

        /// <summary>
        /// Called by the client to notify how many cards were drawn by the player
        /// </summary>
        /// <param name="player">The drawing players</param>
        /// <param name="drawCount">The amount of cards drawn. Can be 0</param>
        public virtual void NotifyPlayerDraws(Player player, int drawCount)
        {
            helper.NotifyPlayerDraws(player, drawCount);
        }

        /// <summary>
        /// Updates the current client with the new cards drawn.
        /// </summary>
        /// <param name="player">
        /// The player with the new updated cards
        /// </param>
        public virtual void NotifyPlayerNewCards(Player player)
        {
            helper.NotifyPlayerNewCards(player);
        }

        /// <summary>
        /// Called by the client to get the player drawing response.
        /// </summary>
        /// <param name="player">The player which draws the cards</param>
        /// <param name="action">The action which should be modified according to the player actions</param>
        public void WaitPlayerDrawingAction(Player player, PlayerDrawingAction action)
        {
            helper.WaitPlayerDrawingAction(player, action);
        }




    }
}
