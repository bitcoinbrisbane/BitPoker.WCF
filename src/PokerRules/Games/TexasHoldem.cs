using System;
using System.Collections.Generic;
using BitPoker.Models.Deck;
using System.Collections.ObjectModel;

namespace PokerRules.Games
{
    /// <summary>
    /// The game class for Texas Hold'em.
    /// </summary>
    /// <remarks>
    /// Texas Hold'em is a game played with 7 cards.
    /// Each player is dealt 2 cards, these are called the Hole. They are unique to each player and can be seen only by the player.
    /// There are 5 community cards which are dealt in the following way:
    /// <list type="bullet">
    /// <item>3 cards at once, this is called the flop</item>
    /// <item>1 card, this is call the turn</item>
    /// <item>Last card, this is called the river</item>
    /// </list>
    /// The player can create the best hand using any combination of the Hole cards &amp; the Community cards.
    /// </remarks>
    public class TexasHoldem : BaseGame
    {
        // The list of community cards.
        private List<Card> communityCards = new List<Card>();
        
        /// <summary>
        /// Gets the dealt community cards.
        /// </summary>
        public ReadOnlyCollection<Card> CommunityCards
        {
            get { return communityCards.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the maximal player count
        /// </summary>
        public override int MaximalPlayersLimit
        {
            // 5 comuunity cards + 2 cards per player
            // (52 - 5) /2 = 23.5
            get { return 23; }
        }

        /// <summary>
        /// Draws a community card out of the top of the deck
        /// </summary>
        /// <returns>
        /// The card which was drawn.
        /// </returns>
        private Card DrawCommunityCard()
        {
            // get next card
            Card result = Deck.Deal();
            // store it in the community cards list
            communityCards.Add(result);
            return result;
        }

        /// <summary>
        /// Called when the game starts, deals all of the player 2 cards. These are the Hole cards.
        /// </summary>
        protected override void OnBeginGame()
        {
            DealToAll(2);
        }

        /// <summary>
        /// Deals the flop, these are the 3 cards which are dealt at once.
        /// </summary>
        public void Flop()
        {
            for (int i = 0; i < 3; ++i)
            {
                // call the base class DealToAll to store the card in each player hand.
                DealToAll(DrawCommunityCard());
            }
        }

        /// <summary>
        /// Deals the turn, this is the card which is dealt after the flop.
        /// </summary>
        public void Turn()
        {
            DealToAll(DrawCommunityCard());
        }

        /// <summary>
        /// Deals the river, this is the last card which is dealt.
        /// </summary>
        public void River()
        {
            DealToAll(DrawCommunityCard());
        }

        /// <summary>
        /// Called when the game ends, does nothing
        /// </summary>
        protected override void OnEndGame()
        {
            
        }
    }
}
