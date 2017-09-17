using System;
using System.Collections.Generic;
using System.Text;

namespace BitPoker.Models.Deck
{
    /// <summary>
    /// Defines the basic deck class. A deck holds a collection of cards which can be dealt.
    /// </summary>
    public class Deck : IEnumerable<Card>
    {
        // used to shuffle
        private static Random rand = new Random();
        // the cards in the dec
        private List<Card> cards = new List<Card>();

        // the next card to deal
        private int deckTop = 0;

        /// <summary>
        /// Creates a new instance of the Deck class.
        /// </summary>
        /// <remarks>
        /// Initially, the deck holds all of the cards sorted by their ordinal value (See <see cref="Card.OrdinalValue"/>).
        /// </remarks>
        public Deck()
        {
            for (int i = 0; i < Card.MAX_ORDINAL_VALUE; ++i)
            {
                cards.Add(Card.FromOrdinalValue(i));
            }
            // release any unused cells.
            cards.TrimExcess();
        }

        /// <summary>
        /// Shuffles the deck of cards to create a new randomized order for the cards.
        /// </summary>
        /// <param name="reshuffleTimes">The number of times to re-shuffle the deck. Must be positive</param>
        /// <remarks>
        /// It is an error to shuffle a deck after a card has been dealt.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Is thrown if the <paramref name="reshuffleTimes"/> is not in the defined range</exception>
        /// <exception cref="InvalidOperationException">Is thrown if the deck has already dealt cards</exception>
        public void Shuffle(int reshuffleTimes)
        {
            if (reshuffleTimes < 0)
                throw new ArgumentOutOfRangeException("reshuffleTimes", "reshuffleTimes must not be negative");
            if (deckTop != 0)
                throw new InvalidOperationException("Can't shuffle after the deck was delt. You can restart first.");
            // use a buffer to shuffle
            List<Card> buffer = new List<Card>();
            for (int j = 0; j < reshuffleTimes; ++j)
            {
                buffer.AddRange(cards);
                for (int i = 0; i < cards.Count; ++i)
                {
                    // get a random card from the buffer and place it in the i'th position in the deck
                    int nextIndex = rand.Next(buffer.Count);
                    cards[i] = buffer[nextIndex];
                    buffer.RemoveAt(nextIndex);
                }
            }
        }

        /// <summary>
        /// Deals the next card out of the deck.
        /// </summary>
        /// <returns>The next card in the deck</returns>
        /// <remarks>
        /// You can use <see cref="HasCards"/> to check if the deck has more cards to deal.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Is thrown if all of the cards were dealt</exception>
        public Card Deal()
        {
            if (deckTop >= cards.Count)
                throw new InvalidOperationException("All of the cards were delt");
            return cards[deckTop++];
        }

        /// <summary>
        /// Determines if the deck has anymore cards to deal.
        /// </summary>
        public bool HasCards { get { return deckTop < cards.Count; } }

        /// <summary>
        /// Restarts the deck dealing order. The deck is not suffled, and the exact same order of cards will be dealt.
        /// </summary>
        public void RestartDealing()
        {
            deckTop = 0;
        }

        #region IEnumerable<Card> Members

        /// <summary>
        /// Returns an enumerator that iterates through the deck cards.
        /// </summary>
        /// <returns>An enumerator that iterates through the deck cards.</returns>
        public IEnumerator<Card> GetEnumerator()
        {
            return cards.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the deck cards.
        /// </summary>
        /// <returns>An enumerator that iterates through the deck cards.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return cards.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Gets the card placed in the given index in the deck. (Can see cards which were already dealt)
        /// </summary>
        /// <param name="index">The index of card to get, must be in the range [0-<see cref="Card.MAX_ORDINAL_VALUE"/>)</param>
        /// <returns>The card placed in the given index in the deck.</returns>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0. -or- index is equal to or greater than <see cref="Card.MAX_ORDINAL_VALUE"/></exception>
        public Card this[int index]
        {
            get { return cards[index]; }
        }

        /// <summary>
        /// Gets the number of cards remains to be dealt.
        /// </summary>
        public int Count { get { return cards.Count - deckTop; } }
    }
}
