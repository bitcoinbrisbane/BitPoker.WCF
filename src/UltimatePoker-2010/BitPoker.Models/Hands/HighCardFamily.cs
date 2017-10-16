using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BitPoker.Models.Deck;

namespace BitPoker.Models.Hands
{
	/// <summary>
	/// Represents a hand family of High Card.
	/// <remarks>a High card hand, is a hand in which the highest combination is the highest card.</remarks>
	/// </summary>
    public class HighCardFamily : HandFamily
    {
		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="HighCardFamily"/> class, with a zero family value.</para>
		/// </summary>
        public HighCardFamily() : base(0) { }

		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="HighCardFamily"/> class.</para>
		/// </summary>
		/// <param name="familyValue">
		///		a Numeric value for the family, to represent its stregth in the current poker rules.
		/// </param>
        public HighCardFamily(int familyValue) : base(familyValue) { }

		/// <summary>
		/// Tries to create a High Card <see cref="Hand"/> from the given cards.
		/// </summary>
		/// <param name="cards">The cards from which to create the hand.</param>
		/// <returns>
		///	a <see cref="Hand"/> of the family's type, or null if the provided cards <br/>
		///	do not contain the specific hand.
		/// </returns>
        protected override Hand GetBestHandOverride(List<Card> cards)
        {
            if (cards.Count < 1)
                return null;
            
			// Narrow the hand down to five cards
            while (cards.Count > 5)
                cards.Remove(cards.Min());
            
			// Make the first card the highest
            cards.Sort();
            cards.Reverse();

            return new HighCardHand(cards, this);
        }

		/// <summary>
		/// Gets the high card from a provided <see cref="Hand"/>.
		/// </summary>
		/// <param name="hand">
		/// The hand from which to get the high card, presumes this hand is of type <see cref="HighCardHand"/>.
		/// </param>
		/// <returns>
		/// The highest card in the hand.
		/// </returns>
        internal static Card GetHighCard(Hand hand)
        {
            HighCardHand realHand = (HighCardHand)hand;
            return realHand.cards[0];
        }

		/// <summary>
		/// Gets the first specific amount if high cards from the hand.
		/// </summary>
		/// <param name="hand">
		/// The hand from which to get the high card, presumes this hand is of type <see cref="HighCardHand"/>.
		/// </param>
		/// <param name="cardCount">The amount of high cards to get.</param>
		/// <returns>
		/// a Specific amount of high cards from the provided hand, if there are not enough cards in the hand <br/>
		/// empty cards will be added.
		/// </returns>
        internal static IEnumerable<Card> GetHighCards(Hand hand, int cardCount)
        {
            HighCardHand realHand = (HighCardHand)hand;
            List<Card> clone = new List<Card>(realHand.cards);
            while (clone.Count < cardCount)
                clone.Add(Card.Empty);
            return clone.Take(cardCount);
        }

		/// <summary>
		/// Represents a High Card Hand
		/// </summary>
        private class HighCardHand : Hand
        {
			/// <summary>
			/// The cards of the hand.
			/// </summary>
            public List<Card> cards;

			/// <summary>
			/// 	<para>Initializes an instance of the <see cref="HighCardHand"/> class.</para>
			/// </summary>
			/// <param name="cards">
			///		The cards from which to create the hand.
			/// </param>
			/// <param name="highCardFamily">
			///		The high card family to use.
			/// </param>
            public HighCardHand(List<Card> cards, HighCardFamily highCardFamily)
                : base(highCardFamily)
            {
                this.cards = cards;
            }

			/// <summary>
			/// Gives a string representation of the hand.
			/// </summary>
			/// <returns>
			/// a String that represents this hand.
			/// </returns>
            public override string ToString()
            {
                return "High Card";
            }

			/// <summary>
			/// Returns an enumerator that iterates only through the cards of the hand.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate only through the cards of the hand.
			/// </returns>
            public override IEnumerator<Card> GetEnumerator()
            {
                return cards.Take(1).GetEnumerator();
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
