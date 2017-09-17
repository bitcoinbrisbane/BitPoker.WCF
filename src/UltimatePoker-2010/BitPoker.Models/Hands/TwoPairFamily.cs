using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BitPoker.Models.Deck;

namespace BitPoker.Models.Hands
{
	/// <summary>
	/// Represents a hand family of Two Pair.
	/// <remarks>a Two Pair hand, is a hand in which the highest combination is Two pairs of cards of the same value.</remarks>
	/// </summary>
    public class TwoPairFamily : HandFamily
    {
        private TwoOfAKindFamily twoOfAKindHelper = new TwoOfAKindFamily();
        private HighCardFamily highCardHelper = new HighCardFamily();

		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="HighCardFamily"/> class, with a zero family value.</para>
		/// </summary>
        public TwoPairFamily() : base(0) { }

		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="HighCardFamily"/> class.</para>
		/// </summary>
		/// <param name="familyValue">
		///		a Numeric value for the family, to represent its stregth in the current poker rules.
		/// </param>
        public TwoPairFamily(int familyValue) : base(familyValue) { }

		/// <summary>
		/// Tries to create a Two Pair <see cref="Hand"/> from the given cards.
		/// </summary>
		/// <param name="cards">The cards from which to create the hand.</param>
		/// <returns>
		///	a <see cref="Hand"/> of the family's type, or null if the provided cards <br/>
		///	do not contain the specific hand.
		/// </returns>
        protected override Hand GetBestHandOverride(List<Card> cards)
        {
            if (cards.Count < 4)
                return null;

            List<Card> clone = new List<Card>();
            clone.AddRange(cards);
            Hand firstHand = twoOfAKindHelper.GetBestHand(clone); // gets the first pair in the hand
            if (firstHand != null)
            {
                KeyValuePair<Card, Card> firstPair = TwoOfAKindFamily.GetPair(firstHand);

                RemoveCardValue(clone,firstPair.Key); // remove the pair from the cards collection

                Hand secondHand = twoOfAKindHelper.GetBestHand(clone); // get another pair
                if (secondHand != null)
                {
                    firstPair = TwoOfAKindFamily.GetPair(secondHand);

					RemoveCardValue(clone, firstPair.Key); // remove the pair from the cards collection

                    Hand helpingHand = highCardHelper.GetBestHand(clone); // get the remaining high card
                    Card highCard = Card.Empty;
                    if (helpingHand != null)
                    {
                        highCard = HighCardFamily.GetHighCard(helpingHand);
                    }

                    return new TwoPairHand(firstHand, secondHand, highCard, this);
                }
            }
            return null;
        }

		/// <summary>
		/// Represents a Two Pair hand.
		/// </summary>
        private class TwoPairHand : Hand
        {
            private List<Card> cards = new List<Card>();

			/// <summary>
			/// 	<para>Initializes an instance of the <see cref="TwoPairHand"/> class.</para>
			/// </summary>
			/// <param name="firstHand">
			///		The first pair in the hand.
			/// </param>
			/// <param name="secondHand">
			///		The second pair in the hand.
			/// </param>
			/// <param name="highCard">
			///		The remaining high card.
			/// </param>
			/// <param name="family">
			///		The creating hand family.
			/// </param>
            public TwoPairHand(Hand firstHand, Hand secondHand, Card highCard, TwoPairFamily family)
                : base(family)
            {
                KeyValuePair<Card, Card> firstPair = TwoOfAKindFamily.GetPair(firstHand);
                KeyValuePair<Card, Card> secondPair = TwoOfAKindFamily.GetPair(secondHand);
                if (firstPair.Key < secondPair.Key)
                {
                    KeyValuePair<Card, Card> swap = secondPair;
                    secondPair = firstPair;
                    firstPair = swap;
                }

                cards.Add(firstPair.Key);
                cards.Add(firstPair.Value);
                cards.Add(secondPair.Key);
                cards.Add(secondPair.Value);

                cards.Add(highCard);
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
                return "Two Pair";
            }
        }
    }
}
