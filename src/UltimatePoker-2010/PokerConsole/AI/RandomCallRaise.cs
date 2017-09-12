using System;
using System.Collections.Generic;
using System.Text;

namespace PokerConsole.AI
{
    /// <summary>
    /// A sofisticated, yet simple aggressive strategy. The automated player doesn't look at the hand, it simply calls 
    /// or raises.
    /// </summary>
    public class RandomCallRaise : AIStrategy
    {
        // the percentage of time in which the player will call. The remainder of the times it will raise.
        private int callPercent;
        // the strategy random number generator
        private Random rand = new Random();

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="RandomCallRaise"/> class.</para>
        /// </summary>
        /// <param name="callPercent">A value between 0-100 which indicates the percentage of times the player will call.
        /// The rest of the time the player will raise
        /// </param>
        public RandomCallRaise(int callPercent)
        {
            this.callPercent = callPercent;
        }

        /// <summary>
        /// Called by the client when a bet decision should be made. 
        /// </summary>
        /// <param name="player">The automated player. 
        /// </param>
        /// <param name="action">The betting action which must be modified to pass the client response</param>
        public override void Bet(PokerEngine.Player player, PokerEngine.Engine.PlayerBettingAction action)
        {
            int decision = rand.Next(100);
            // the decision is with in the call percent range, call:
            if (decision < callPercent)
                action.Call(); 
            else
                // out of the range, raise!
                action.Raise(action.RaiseAmount);
        }
    }
}
