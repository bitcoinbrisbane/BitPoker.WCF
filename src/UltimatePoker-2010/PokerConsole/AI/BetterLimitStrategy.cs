using System;
using System.Collections.Generic;
using System.Text;
using BitPoker.Models.Hands;
using PokerEngine;
using PokerConsole.Engine;
using PokerEngine.Engine;
using PokerRules.Games;

namespace PokerConsole.AI
{
    /// <summary>
    /// An improved strategy which holds a bet limit and looks at the hand value
    /// </summary>
    public class BetterLimitStrategy : AIStrategy
    {
        // the limit which below it the strategy will fold
        private int preparedLimit;
        // The owning client which will provide hand knowledge
        private IRulesInterpreter client;
        // the number of times the synchronization was called. It is used to swith the client to an
        // "aggressive mode" when the hand is valuable.
        private int synchTimes = 0;

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="BetterLimitStrategy"/> class.</para>
        /// </summary>
        /// <param name="preparedLimit">The amount of money which marks the lower limit for the strategy. 
        /// </param>
        /// <param name="client">The ownining client
        /// </param>
        public BetterLimitStrategy(int preparedLimit, IRulesInterpreter client)
        {
            this.preparedLimit = preparedLimit;
            this.client = client;
        }

        /// <summary>
        /// Called by the client when an update arrives. Override this method to add logic for seeing the cards played
        /// </summary>
        /// <param name="syhcronizationData">The players updated. Must not be null</param>
        public override void Synchronize(IEnumerable<Player> syhcronizationData)
        {
            base.Synchronize(syhcronizationData);
            ++synchTimes;
        }

        /// <summary>
        /// Called by the client when a bet decision should be made. 
        /// </summary>
        /// <param name="player">The automated player. 
        /// </param>
        /// <param name="action">The betting action which must be modified to pass the client response</param>
        /// <remarks>
        /// The limit strategy looks at the hand value first. If it is low, 
        /// the player will fold when the money needed to bet is below the limit. When the hand value is high, the player will call for 
        /// the first couple of times and will raise for the rest
        /// </remarks>
        public override void Bet(Player player, PlayerBettingAction action)
        {
            // check to see the player hand
            Hand hand = client.GetBestHand(player.Cards);
            // the hand good, call at first and raise near the end.
            if (hand.Family.FamilyValue > 3)
            {
                if (synchTimes < 5)
                    action.Call();
                else
                    action.Raise(action.RaiseAmount);
            }
            else // the hand is bad, watch for the limit
            {
                if (action.CallAmount > 0 && player.Money - action.CallAmount < preparedLimit)
                    action.Fold();
                else
                    action.Call();
            }
        }

    }
}
