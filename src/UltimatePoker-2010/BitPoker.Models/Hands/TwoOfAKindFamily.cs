using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitPoker.Models.Deck;

namespace BitPoker.Models.Hands
{
	/// <summary>
	/// Represents a hand family of Two of a Kind.
	/// <remarks>a Two of a Kind hand, is a hand in which the highest combination is two cards of the same value.</remarks>
	/// </summary>
    public class TwoOfAKindFamily : HandFamily
    {

		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="TwoOfAKindFamily"/> class, with a zero family value.</para>
		/// </summary>
        public TwoOfAKindFamily() : base(0) { }

		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="TwoOfAKindFamily"/> class.</para>
		/// </summary>
		/// <param name="familyValue">
		///		a Numeric value for the family, to represent its stregth in the current poker rules.
		/// </param>
        public TwoOfAKindFamily(int familyValue) : base(familyValue) { }

		/// <summary>
		/// Tries to create a Two of a Kind <see cref="Hand"/> from the given cards.
		/// </summary>
		/// <param name="cards">The cards from which to create the hand.</param>
		/// <returns>
		///	a Two of a Kind hand, or null if the provided cards do not contain the specific hand.
		/// </returns>
        protected override Hand GetBestHandOverride(List<Card> cards)
        {
            List<Card> pair = NofAKindFamily.GetHighestCards(cards, 2);
            if (pair != null)
            {
                IEnumerable<Card> highCards = NofAKindFamily.GetHighCards(cards, pair);
                return new TwoOfAKindHand(pair, highCards, this);
            }
            return null;
        }

		/// <summary>
		/// Returns the pair in the provided <see cref="Hand"/>
		/// </summary>
		/// <param name="hand">The provided hand, presumes this hand is of type <see cref="TwoOfAKindHand"/>.</param>
		/// <returns>The first pair of cards in the hand</returns>
        internal static KeyValuePair<Card, Card> GetPair(Hand hand)
        {
            TwoOfAKindHand realHand = (TwoOfAKindHand)hand;
            return new KeyValuePair<Card, Card>(realHand.cards[0], realHand.cards[1]);
        }

		/// <summary>
		/// Gets the first high card ater the pair, in the provided hand
		/// </summary>
		/// <param name="hand">The provided hand, presumes this hand is of type <see cref="TwoOfAKindHand"/>.</param>
		/// <returns>The first high card ater the pair, empty card if no such card exists.</returns>
        internal static Card GetHighCard(Hand hand)
        {
            TwoOfAKindHand realHand = (TwoOfAKindHand)hand;
            if (!realHand.cards[0].AreOfSameNumericValue(realHand.cards[2]))
                return realHand.cards[2];
            return Card.Empty;
        }

		/// <summary>
		/// Represents a Two of a Kind hand.
		/// </summary>
        private class TwoOfAKindHand : Hand
        {
            /// <summary>
            /// Gets the hand cards
            /// </summary>
            public List<Card> cards = new List<Card>();

			/// <summary>
			/// 	<para>Initializes an instance of the <see cref="TwoOfAKindHand"/> class.</para>
			/// </summary>
			/// <param name="pair">
			///		The pair of the hands (two of a kind).
			/// </param>
			/// <param name="highCards">
			///		The rest of the cards in the hand.
			/// </param>
			/// <param name="family">
			///		The creating hand family
			/// </param>
            public TwoOfAKindHand(List<Card> pair, IEnumerable<Card> highCards, TwoOfAKindFamily family)
                : base(family)
            {
                this.cards.AddRange(pair);

                cards.AddRange(highCards);
            }

			/// <summary>
			/// Gives a string representation of the hand.
			/// </summary>
			/// <returns>
			/// a String that represents this hand.
			/// </returns>
            public override string ToString()
            {
                return "A Pair";
            }

			/// <summary>
			/// Returns an enumerator that iterates only through the cards of the hand.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate only through the cards of the hand.
			/// </returns>
            public override IEnumerator<Card> GetEnumerator()
            {
                return cards.Take(2).GetEnumerator();
            }

			/// <summary>
			/// Returns an enumerator that iterates through all the player's cards, not just the hand cards.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate only through all the player's cards.
			/// </returns>
            public override IEnumerator<Card> GetAllCards()
            {
                return cards.GetEnumerator();
            }
        }
    }
}
