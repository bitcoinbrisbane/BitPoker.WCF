using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerEngine;
using PokerEngine.Engine;
using PokerConsole.Engine;
using PokerRules.Hands;
using PokerRules.Deck;
using PokerRules.Games;

namespace PokerConsole.AI
{
    /// <summary>
    /// A simple 5 card draw strategy which looks at the current hand of the player and decides if to call or raise
    /// </summary>
    public class SimpleFiveCardStrategy : AIStrategy
    {
        // the client is used to determine the player hand
        private IRulesInterpreter client;
        // the last detected name
        private Hand lastHand = null;

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="SimpleFiveCardStrategy"/> class.</para>
        /// </summary>
        /// <param name="client">The client which uses the strategy. It is used to determine the automated player hand.
        /// </param>
        public SimpleFiveCardStrategy(IRulesInterpreter client)
        {
            this.client = client;
        }



        /// <summary>
        /// Called by the client when a bet decision should be made.
        /// </summary>
        /// <param name="player">The automated player. 
        /// </param>
        /// <param name="action">The betting action which must be modified to pass the client response</param>
        public override void Bet(Player player, PlayerBettingAction action)
        {
            // didn't see the hand yet, stick around for the draw
            if (lastHand == null)
            {
                action.Call();
            }
            else
            {
                // check the hand value
                if (lastHand.Family.FamilyValue > 3)
                    action.Raise(action.RaiseAmount); // high value-> raise
                else
                    action.Call(); // low value -> call
            }
        }

        /// <summary>
        /// Called by a client which needs to manually draw cards. Draws the cards which are not part of the best hand.
        /// </summary>
        /// <param name="player">The automated player</param>
        /// <param name="action">The drawing action which must be modified to pass the strategy decision</param>
        public override void Draw(Player player, PlayerDrawingAction action)
        {
            // get the player best hand
            lastHand = client.GetBestHand(player.Cards);
            // the other cards holder
            List<Card> otherCards = new List<Card>();
            // get all of the hand cards
            IEnumerator<Card> allCards = lastHand.GetAllCards();
            while (allCards.MoveNext())
            {
                if (!lastHand.Contains(allCards.Current))
                {
                    otherCards.Add(allCards.Current);
                }
            }
            // sort the other cards by value, the order is ascending 
            otherCards.Sort();
            // can draw at most 3 cards:
            int max = Math.Min(3, otherCards.Count);
            // draw the first (max) cards out of the other cards:
            action.DrawnCards.AddRange(otherCards.Take(max));
        }
    }
}
