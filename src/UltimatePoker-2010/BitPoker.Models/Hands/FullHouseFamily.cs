using System;
using System.Collections.Generic;
using BitPoker.Models.Deck;

namespace BitPoker.Models.Hands
{
	/// <summary>
	/// Represents a hand family of Full House.
	/// <remarks>a Full House hand, is a hand in which the highest combination is a Three of a kind and a Pair.</remarks>
	/// </summary>
    public class FullHouseFamily : HandFamily
    {
        private ThreeOfAKindFamily threeOfAKindHelper = new ThreeOfAKindFamily();
        private TwoOfAKindFamily twoOfAKindHelper = new TwoOfAKindFamily();

		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="FullHouseFamily"/> class, with a zero family value.</para>
		/// </summary>
        public FullHouseFamily() : base(0) { }

		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="FullHouseFamily"/> class.</para>
		/// </summary>
		/// <param name="familyValue">
		///		a Numeric value for the family, to represent its stregth in the current poker rules.
		/// </param>
        public FullHouseFamily(int familyValue) : base(familyValue) { }

		/// <summary>
		/// Tries to create a Full House <see cref="Hand"/> from the given cards.
		/// </summary>
		/// <param name="cards">The cards from which to create the hand.</param>
		/// <returns>
		///	a <see cref="Hand"/> of the family's type, or null if the provided cards <br/>
		///	do not contain the specific hand.
		/// </returns>
        protected override Hand GetBestHandOverride(List<Card> cards)
        {
			// get the three of a kind
            Hand threesom = threeOfAKindHelper.GetBestHand(cards);
            if (threesom != null)
            {
                List<Card> clone = new List<Card>();

                clone.AddRange(cards);

                IEnumerator<Card> enumerator = threesom.GetEnumerator();
                enumerator.MoveNext();

				// remove the three of a kind from clone list
                RemoveCardValue(clone, enumerator.Current); 

				// get the pair
                Hand pair = twoOfAKindHelper.GetBestHand(clone);
                if (pair != null)
                {
                    return new FullHouseHand(threesom, pair, this);
                }
            }
            return null;
        }

		/// <summary>
		/// Represents a Full House hand.
		/// </summary>
        private class FullHouseHand : Hand
        {
            private List<Card> cards = new List<Card>();

			/// <summary>
			/// 	<para>Initializes an instance of the <see cref="FullHouseHand"/> class.</para>
			/// </summary>
			/// <param name="threesome">
			///		The Three of a kind in the full house.
			/// </param>
			/// <param name="pair">
			///		The Pair in the full house.
			/// </param>
			/// <param name="family">
			///		The creating hand family.
			/// </param>
            public FullHouseHand(Hand threesome, Hand pair, FullHouseFamily family)
                : base(family)
            {
                cards.AddRange(ThreeOfAKindFamily.GetThreesom(threesome));
                KeyValuePair<Card, Card> pairCards = TwoOfAKindFamily.GetPair(pair);
                cards.Add(pairCards.Key);
                cards.Add(pairCards.Value);
            }

			/// <summary>
			/// Returns an enumerator that iterates only through the cards of the hand.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate only through the cards of the hand.
			/// </returns>
            public override IEnumerator<Card> GetEnumerator()
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
                return "Full House";
            }

			/// <summary>
			/// Returns an enumerator that iterates through all the player's cards, not just the hand cards.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate only through all the player's cards.
			/// </returns>
            public override IEnumerator<Card> GetAllCards()
            {
                return GetEnumerator();
            }
        }
    }
}
