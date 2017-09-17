using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitPoker.Models.Deck;
using BitPoker.Models.Hands;
using System.Collections.ObjectModel;

namespace PokerRules.Games
{
    /// <summary>
    /// The game class for Omaha Hold'em
    /// </summary>
    /// <remarks>
    /// Omaha Hold'em is a variation of Texas hold'em where each hand must be composed out of exactly 2 cards 
    /// out of the Hole cards, and exactly 3 cards out of the community cards.
    /// </remarks>
    public class OmahaHoldem : TexasHoldem
    {
        /// <summary>
        /// Gets the maximal player count
        /// </summary>
        public override int MaximalPlayersLimit
        {
            // 5 comuunity cards + 4 cards per player
            // (52 - 5) /4 = 11.75
            get { return 11; }
        }

        /// <summary>
        /// Called when the game starts, deals all of the player 4 cards. These are the Hole cards.
        /// </summary>
        protected override void OnBeginGame()
        {
            DealToAll(4);
        }

        /// <summary>
        /// Gets the best hand according to this game logic out of the given cards.
        /// </summary>
        /// <param name="clone">A list of cards to search for the best hand, not null and can be modified.</param>
        /// <returns>
        /// The best hand which can be creates using the given cards or null if none exists.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method is overriden to implement the Omaha Hold'em restrictions.
        /// </para>
        /// <para>
        /// The list of cards are considered as the player cards.
        /// </para>
        /// </remarks>
        protected override Hand GetBestHandOverride(List<Card> clone)
        {
            // make sure there are no community cards in the clone.
            List<Card> playerCards = new List<Card>(
                                     from cur in clone
                                     where !ExposedCommunityCards.Contains(cur)
                                     select cur);
            // this is the list of cards which is modified to create all of the possible combinations
            // it must be a list since it is passed to base.GetBestHandOverride
            List<Card> handCards = new List<Card>(5);

            // ignore empty player cards or a list which contains empty cards.
            if (playerCards.Count == 0 || playerCards.TrueForAll((card) => card == Card.Empty))
                return null;

            // setup the hand cards
            for (int i = 0; i < handCards.Capacity; ++i)
                handCards.Add(Card.Empty);

            // a place holder for the best hand
            Hand bestHand = null;

            // Here, the hand cards are calculated for all of the possibilities.
            // The order of the player cards does not matter.
            // First, select all possible combinations of 2 cards out of the player cards
            // Second, select all possible combinations of 3 cards out of the community cards
            // If no community cards exist, get the best hand using only the player cards.
            for (int i = 0; i < playerCards.Count; ++i)
            {
                handCards[0] = playerCards[i];
                // skip the cards selected by the external loop
                for (int j = i + 1; j < playerCards.Count; ++j)
                {
                    handCards[1] = playerCards[j];


                    // check if there are community cards
                    if (ExposedCommunityCards.Count > 0)
                    {
                        // select all possible combinations of the community cards
                        for (int k = 0; k < ExposedCommunityCards.Count; ++k)
                        {
                            handCards[2] = ExposedCommunityCards[k];

                            for (int l = k + 1; l < ExposedCommunityCards.Count; ++l)
                            {
                                handCards[3] = ExposedCommunityCards[l];

                                for (int m = l + 1; m < ExposedCommunityCards.Count; ++m)
                                {
                                    handCards[4] = ExposedCommunityCards[m];

                                    // use current combination and if the generated hand 
                                    // is better than the current best hand replace them.
                                    checkReplaceHand(handCards, ref bestHand);

                                    handCards[4] = Card.Empty; // clear the card
                                }
                                handCards[3] = Card.Empty;
                            }
                            handCards[2] = Card.Empty;
                        }
                    }
                    else // no community cards, use only the player hands
                        checkReplaceHand(handCards, ref bestHand);
                    handCards[1] = Card.Empty;
                }
                handCards[0] = Card.Empty;
            }
            return bestHand;
        }
        /// <summary>
        /// This helper method calcualtes the best hand using the base class implementation.
        /// </summary>
        /// <param name="cards">The list or cards to search for the best hand</param>
        /// <param name="bestHand">
        /// The reference to the current best hand
        /// </param>
        private void checkReplaceHand(List<Card> cards, ref Hand bestHand)
        {
            // try get the best hand using the base class implementation
            Hand curHand = base.GetBestHandOverride(cards);
            // if there is no best hand, use this hand in any case.
            if (bestHand == null)
                bestHand = curHand;
            // there is a best hand, compare them and replace only if the curHand is better than the best hand.    
            else if (curHand != null && curHand.CompareTo(bestHand) > 0)
                bestHand = curHand;
        }

        private ReadOnlyCollection<Card> exposedCommunityCards;
        /// <summary>
        /// Gets or sets the community cards.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Since the calculation of the best hand relies on the community cards, it should be exposed and allow set for
        /// client side applications. Servers will send the community cards and the clients will need to retrieve them and set it here.
        /// </para>
        /// <para>
        /// This property will never return null, if there were no community cards set, the base <see cref="TexasHoldem.CommunityCards"/> will be used.
        /// </para>
        /// </remarks>
        public ReadOnlyCollection<Card> ExposedCommunityCards
        {
            get
            {
                // use the exposedCommunityCards if it isn't null
                if (exposedCommunityCards != null) 
                    return exposedCommunityCards;
                return CommunityCards;
            }
            set { exposedCommunityCards = value; }

        }
    }
}
