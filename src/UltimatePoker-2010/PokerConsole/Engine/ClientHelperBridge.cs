using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerEngine;
using PokerEngine.Engine;
using PokerService;
using PokerRules.Deck;

namespace PokerConsole.Engine
{
    /// <summary>
    /// A bridge which is used to modify an active <see cref="IClientHelper"/> or 
    /// a <see cref="IFiveCardClientHelper"/>
    /// </summary>
    /// <remarks>
    /// Users of this class must use either <see cref="ClientHelper"/> or <see cref="FiveCardHelper"/>
    /// to set a valid helper
    /// </remarks>
    public class ClientHelperBridge : ClientHelperDecorator, IFiveCardClientHelper
    {
        // The five card helper which is used
        private IFiveCardClientHelper fiveCardHelper;

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="ClientHelperBridge"/> class.</para>
        /// </summary>
        public ClientHelperBridge()
            : base(null) // pass null, users must set a helper
        {

        }

        /// <summary>
        /// Gets or sets the client helper which is used
        /// </summary>
        public IClientHelper ClientHelper
        {
            get { return base.Helper; }
            set { base.Helper = value; }
        }

        /// <summary>
        /// Gets or sets the five card helper which is used. Also modifies <see cref="ClientHelper"/>
        /// </summary>
        public IFiveCardClientHelper FiveCardHelper
        {
            get { return fiveCardHelper; }
            set
            {
                base.Helper = value;
                fiveCardHelper = value;
            }
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

        /// <summary>
        /// Called by the client to get the player drawing response.
        /// </summary>
        /// <param name="player">The player which draws the cards</param>
        /// <param name="action">The action which should be modified according to the player actions</param>
        public void WaitPlayerDrawingAction(Player player, PlayerDrawingAction action)
        {
            fiveCardHelper.WaitPlayerDrawingAction(player, action);
        }

        #endregion
    }
}
