using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerConsole.Engine;
using PokerEngine;
using PokerRules.Deck;
using PokerEngine.Engine;
using PokerRules.Games;

namespace PokerConsole.AI
{
    /// <summary>
    /// A simple automated five card draw client
    /// </summary>
    public class AiFiveCardDrawClient : AiClientHelper, IFiveCardClientHelper
    {
        private IFiveCardClientHelper fiveCardHelper;
        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="AiFiveCardDrawClient"/> class.</para>
        /// </summary>
        /// <param name="helper">The concrete helper which is used for unimportant I/O</param>
        /// <param name="interpreter">The rules interpreter which is used by the automated client</param>
        public AiFiveCardDrawClient(IFiveCardClientHelper helper, IRulesInterpreter interpreter)
            : base(helper, interpreter)
        {
            this.fiveCardHelper = helper;
        }




        #region IFiveCardClientHelper Members

        /// <summary>
        /// Called by the client when a drawing round starts
        /// </summary>
        public void NotifyDrawingRoundStarted()
        {
            fiveCardHelper.NotifyDrawingRoundStarted();
        }

        /// <summary>
        /// Called by the client when a drawing round completes
        /// </summary>
        public void NotifyDrawingRoundCompleted()
        {
            fiveCardHelper.NotifyDrawingRoundCompleted();
        }

        /// <summary>
        /// Called by the client to notify how many cards were drawn by the player
        /// </summary>
        /// <param name="player">The drawing players</param>
        /// <param name="drawCount">The amount of cards drawn. Can be 0</param>
        public void NotifyPlayerDraws(Player player, int drawCount)
        {
            fiveCardHelper.NotifyPlayerDraws(player, drawCount);
        }

        /// <summary>
        /// Updates the current client with the new cards drawn.
        /// </summary>
        /// <param name="player">
        /// The player with the new updated cards
        /// </param>
        public void NotifyPlayerNewCards(Player player)
        {
            fiveCardHelper.NotifyPlayerNewCards(player);
        }

        #endregion
    }
}
