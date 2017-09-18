using System;
using System.Collections.Generic;
using System.Linq;
using BitPoker.Models.Deck;

namespace BitPoker.Models.Hands
{
    /// <summary>
    /// Represents a hand family of Four of a Kind.
    /// <remarks>a Four of a Kind hand, is a hand in which the highest combination is three cards of the same value.</remarks>
    /// </summary>
    public class FourOfAKindFamily : HandFamily
    {
		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="FourOfAKindHand"/> class, with a zero family value.</para>
		/// </summary>
        public FourOfAKindFamily() : base(0) { }

		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="FourOfAKindHand"/> class.</para>
		/// </summary>
		/// <param name="familyValue">
		///		a Numeric value for the family, to represent its stregth in the current poker rules.
		/// </param>
        public FourOfAKindFamily(int familyValue) : base(familyValue) { }

		/// <summary>
		/// Tries to create a Four of a Kind <see cref="Hand"/> from the given cards.
		/// </summary>
		/// <param name="cards">The cards from which to create the hand.</param>
		/// <returns>
		///	a Four of a Kind hand, or null if the provided cards do not contain the specific hand.
		/// </returns>
        protected override Hand GetBestHandOverride(List<Card> cards)
        {
			List<Card> foursom = NofAKindFamily.GetHighestCards(cards, 4); // get the four of a kind
            if (foursom != null)
            {
				// gets the rest of the cards in the hand
                IEnumerable<Card> highCards = NofAKindFamily.GetHighCards(cards, foursom);
                return new FourOfAKindHand(foursom, highCards, this);
            }
            return null;
        }

		/// <summary>
		/// Represents a Four of a kind Hand
		/// </summary>
        private class FourOfAKindHand : Hand
        {
            private List<Card> cards;

			/// <summary>
			/// 	<para>Initializes an instance of the <see cref="FourOfAKindHand"/> class.</para>
			/// </summary>
			/// <param name="foursom">
			///		The four cards of the same value.
			/// </param>
			/// <param name="highCard">
			///		The remaining card in the hand.
			/// </param>
			/// <param name="family">
			///		The creating hand family.
			/// </param>
            public FourOfAKindHand(List<Card> foursom, IEnumerable<Card> highCard, FourOfAKindFamily family)
                : base(family)
            {
                this.cards = foursom;
                this.cards.AddRange(highCard);
            }

			/// <summary>
			/// Returns an enumerator that iterates only through the cards of the hand.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate only through the cards of the hand.
			/// </returns>
            public override IEnumerator<Card> GetEnumerator()
            {
                return cards.Take(4).GetEnumerator();
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

			/// <summary>
			/// Gives a string representation of the hand.
			/// </summary>
			/// <returns>
			/// a String that represents this hand.
			/// </returns>
            public override string ToString()
            {
                return "Four of a Kind";
            }
        }
    }
}
