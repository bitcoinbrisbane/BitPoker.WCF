using System;
using System.Collections.Generic;
using System.Text;
using PokerEngine;
using PokerEngine.Engine;

namespace PokerConsole.AI
{
    /// <summary>
    /// A very simple strategy which holds a limit. When the total bet is below the defined limit the automated player will fold
    /// </summary>
    public class SimpleLimitStrategy : AIStrategy
    {
        // the defined lower limit
        private int preparedLimit;


        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="SimpleLimitStrategy"/> class.</para>
        /// </summary>
        /// <param name="preparedLimit">The limit which defines the lower bound. Beneath this bound the player will fold.
        /// </param>
        public SimpleLimitStrategy(int preparedLimit)
        {
            this.preparedLimit = preparedLimit;
        }


        /// <summary>
        /// Called by the client when a bet decision should be made. 
        /// </summary>
        /// <param name="player">The automated player. 
        /// </param>
        /// <param name="action">The betting action which must be modified to pass the client response</param>
        /// <remarks>
        /// Looks at the current bet and folds if the bet amount will reduce the player money below the deined limit.
        /// </remarks>
        public override void Bet(Player player, PlayerBettingAction action)
        {

            if (action.CallAmount > 0 && player.Money - action.CallAmount < preparedLimit)
                action.Fold();
            else
                action.Call();
        }
    }
}
