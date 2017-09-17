using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BitPoker.Models.Deck;

namespace BitPoker.Models.Hands
{
	/// <summary>
	/// Represents a hand family of Three of a Kind.
	/// <remarks>a Three of a Kind hand, is a hand in which the highest combination is three cards of the same value.</remarks>
	/// </summary>
    public class ThreeOfAKindFamily : HandFamily
    {
        private HighCardFamily highCardHelper = new HighCardFamily();

		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="ThreeOfAKindFamily"/> class, with a zero family value.</para>
		/// </summary>
        public ThreeOfAKindFamily() : base(0) { }

		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="ThreeOfAKindFamily"/> class.</para>
		/// </summary>
		/// <param name="familyValue">
		///		a Numeric value for the family, to represent its stregth in the current poker rules.
		/// </param>
        public ThreeOfAKindFamily(int familyValue) : base(familyValue) { }

		/// <summary>
		/// Tries to create a Three of a Kind <see cref="Hand"/> from the given cards.
		/// </summary>
		/// <param name="cards">The cards from which to create the hand.</param>
		/// <returns>
		///	a Three of a Kind hand, or null if the provided cards do not contain the specific hand.
		/// </returns>
        protected override Hand GetBestHandOverride(List<Card> cards)
        {
            List<Card> triplet = NofAKindFamily.GetHighestCards(cards, 3); // get the three of a kind
            if (triplet != null)
            {
				// gets the rest of the cards in the hand
                IEnumerable<Card> highCards = NofAKindFamily.GetHighCards(cards, triplet);
                return new ThreeOfAKindHand(triplet, highCards, this);
            }
            return null;
        }

		/// <summary>
		/// Gets the three cards that create the Three of a Kind, from the provided hand.
		/// </summary>
		/// <param name="hand">The hand from which to get the high card, presumes this hand is of type <see cref="ThreeOfAKindHand"/>.</param>
		/// <returns></returns>
        internal static Card[] GetThreesom(Hand hand)
        {
            ThreeOfAKindHand realHand = (ThreeOfAKindHand)hand;

            return realHand.Take(3).ToArray();
        }
        
		/// <summary>
		/// Represents a Three of a kind Hand
		/// </summary>
        private class ThreeOfAKindHand : Hand
        {
            private List<Card> cards = new List<Card>();

			/// <summary>
			/// 	<para>Initializes an instance of the <see cref="ThreeOfAKindHand"/> class.</para>
			/// </summary>
			/// <param name="triplet">
			///		The three cards of the same value.
			/// </param>
			/// <param name="highCards">
			///		The rest of the cards in the hand.
			/// </param>
			/// <param name="family">
			///		The creating hand family.
			/// </param>
            public ThreeOfAKindHand(List<Card> triplet, IEnumerable<Card> highCards, ThreeOfAKindFamily family)
                : base(family)
            {
                cards.AddRange(triplet);
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
                return "Three of a Kind";
            }

			/// <summary>
			/// Returns an enumerator that iterates only through the cards of the hand.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate only through the cards of the hand.
			/// </returns>
            public override IEnumerator<Card> GetEnumerator()
            {
                return cards.Take(3).GetEnumerator();
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
