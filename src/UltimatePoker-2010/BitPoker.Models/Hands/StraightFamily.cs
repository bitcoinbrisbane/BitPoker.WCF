using System;
using System.Collections.Generic;
using System.Text;
using BitPoker.Models.Deck;

namespace BitPoker.Models.Hands
{
	/// <summary>
	/// Represents a hand family of Straight.
	/// <remarks>a Straight hand, is a hand in which the highest combination is all cards going up in value by 1.</remarks>
	/// </summary>
    public class StraightFamily : HandFamily
    {
		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="StraightFamily"/> class, with a zero family value.</para>
		/// </summary>
        public StraightFamily() : base(0) { }

		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="StraightFamily"/> class.</para>
		/// </summary>
		/// <param name="familyValue">
		///		a Numeric value for the family, to represent its stregth in the current poker rules.
		/// </param>
        public StraightFamily(int familyValue) : base(familyValue) { }

		/// <summary>
		/// Tries to create a Straight <see cref="Hand"/> from the given cards.
		/// </summary>
		/// <param name="cards">The cards from which to create the hand.</param>
		/// <returns>
		///	a <see cref="Hand"/> of the family's type, or null if the provided cards <br/>
		///	do not contain the specific hand.
		/// </returns>
        protected override Hand GetBestHandOverride(List<Card> cards)
        {
            if (cards.Count < 5)
                return null;

            SortedList<int, Card> sortedCards = new SortedList<int, Card>();

            foreach (Card card in cards) // sort the current cards
            {
                if (!sortedCards.ContainsKey(card.CardValue))
                    sortedCards.Add(card.CardValue, card);
            }

            List<Card> higestStraight = new List<Card>();

            IList<Card> values = sortedCards.Values;


            // TODO - add special case for ace,2,3,4,5 straight:
			// checks if all the cards are adjacent
            for (int i = values.Count - 1; i > -1; --i) 
            {
                higestStraight.Add(values[i]);

                if (higestStraight.Count == 4 && higestStraight[3].AreNumericValuesAdjacent(values[values.Count - 1]))
                    higestStraight.Add(values[values.Count - 1]);

                if (higestStraight.Count == 5)
                    return new StraightHand(higestStraight, this);

                if (i > 0 && !values[i].AreNumericValuesAdjacent(values[i - 1]))
                    higestStraight.Clear();
            }

            return null;

        }

		/// <summary>
		/// Represents a Straight hand.
		/// </summary>
        private class StraightHand : Hand
        {
            private List<Card> cards;


			/// <summary>
			/// 	<para>Initializes an instance of the <see cref="StraightHand"/> class.</para>
			/// </summary>
			/// <param name="cards">
			///		The cards that create the straight.
			/// </param>
			/// <param name="family">
			///		The creating hand family.
			/// </param>
            public StraightHand(List<Card> cards, StraightFamily family)
                : base(family)
            {
                this.cards = cards;
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
			/// Returns an enumerator that iterates through all the player's cards, not just the hand cards.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate only through all the player's cards.
			/// </returns>
            public override IEnumerator<Card> GetAllCards()
            {
                return GetEnumerator();
            }

			/// <summary>
			/// Gives a string representation of the hand.
			/// </summary>
			/// <returns>
			/// a String that represents this hand.
			/// </returns>
            public override string ToString()
            {
                return "Straight";
            }
        }
    }
}
