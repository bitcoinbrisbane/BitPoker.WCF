using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerConsole.Engine;

namespace UltimatePoker.Engine
{
    /// <summary>
    /// A generic wpf helper for GUI based clients which supports five card draw games
    /// </summary>
    public class FiveCardGuiHelper : GuiHelper, IFiveCardClientHelper
    {
        private IFiveCardGuiClientHelper helper;

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="FiveCardGuiHelper"/> class.</para>
        /// </summary>
        /// <param name="helper">The concrete helper which is used to pass concrete calls
        /// </param>
        public FiveCardGuiHelper(IFiveCardGuiClientHelper helper)
            : base(helper)
        {
            this.helper = helper;
        }

        #region IFiveCardClientHelper Members

        /// <summary>
        /// Called by the client when a drawing round starts
        /// </summary>
        public void NotifyDrawingRoundStarted()
        {
            Invoke(helper.NotifyDrawingRoundStarted);
        }

        /// <summary>
        /// Called by the client when a drawing round completes
        /// </summary>
        public void NotifyDrawingRoundCompleted()
        {
            Invoke(helper.NotifyDrawingRoundCompleted);
        }

        /// <summary>
        /// Called by the client to notify how many cards were drawn by the player
        /// </summary>
        /// <param name="player">The drawing players</param>
        /// <param name="drawCount">The amount of cards drawn. Can be 0</param>
        public void NotifyPlayerDraws(PokerEngine.Player player, int drawCount)
        {
            Invoke<PokerEngine.Player, int>(helper.NotifyPlayerDraws, player, drawCount);
        }

        /// <summary>
        /// Updates the current client with the new cards drawn.
        /// </summary>
        /// <param name="player">
        /// The player with the new updated cards
        /// </param>
        public void NotifyPlayerNewCards(PokerEngine.Player player)
        {
            Invoke<PokerEngine.Player>(helper.NotifyPlayerNewCards, player);
        }

        /// <summary>
        /// Called by the client to get the player drawing response.
        /// </summary>
        /// <param name="player">The player which draws the cards</param>
        /// <param name="action">The action which should be modified according to the player actions</param>
        public void WaitPlayerDrawingAction(PokerEngine.Player player, PokerEngine.Engine.PlayerDrawingAction action)
        {
            CheckWaitLimit();
            Invoke<PokerEngine.Player, PokerEngine.Engine.PlayerDrawingAction>(helper.WaitPlayerDrawingAction, player, action);
        }

        #endregion
    }
}
