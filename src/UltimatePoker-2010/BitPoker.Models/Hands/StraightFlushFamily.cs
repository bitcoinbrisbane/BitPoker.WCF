using System;
using System.Collections.Generic;
using System.Text;
using BitPoker.Models.Deck;

namespace BitPoker.Models.Hands
{
	/// <summary>
	/// Represents a hand family of Straight Flush.
	/// <remarks>a Straight hand, is a hand in which the highest combination is all cards going up by a <br/>
	/// value of 1, and have the same suites.</remarks>
	/// </summary>
    public class StraightFlushFamily : HandFamily
    {
        private StraightFamily straightHelper = new StraightFamily();

		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="StraightFlushFamily"/> class, with a zero family value.</para>
		/// </summary>
        public StraightFlushFamily() : base(0) { }

		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="StraightFlushFamily"/> class.</para>
		/// </summary>
		/// <param name="familyValue">
		///		a Numeric value for the family, to represent its stregth in the current poker rules.
		/// </param>
        public StraightFlushFamily(int familyValue) : base(familyValue) { }


		/// <summary>
		/// Tries to create a Straight Flush <see cref="Hand"/> from the given cards.
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
            
			// sort cards by suite
            Suite[] suites = (Suite[])Enum.GetValues(typeof(Suite));
            List<Card>[] sortedCards = new List<Card>[suites.Length];
            for (int i = 0; i < suites.Length; ++i)
                sortedCards[i] = new List<Card>();
            foreach (Card card in cards)
            {
                int index = Array.IndexOf<Suite>(suites, card.CardSuite);
                sortedCards[index].Add(card);
            }

			// try and get a straight from each available suite
            Hand bestHand = null;
            for (int i = 0; i < sortedCards.Length; ++i)
            {
                Hand curHand = straightHelper.GetBestHand(sortedCards[i]);
                if (bestHand == null)
                    bestHand = curHand;
                else if (curHand != null)
                {
                    if (curHand.CompareTo(bestHand) > 0) // keep the best straight flush
                        bestHand = curHand;
                }
            }
            if (bestHand != null)
            {
                bestHand = new StraighFlushHand(bestHand, this);
            }
            return bestHand;
           
        }

		/// <summary>
		/// Represents a Straight Flush hand.
		/// </summary>
        private class StraighFlushHand : Hand
        {
            private Hand originalHand;

			/// <summary>
			/// 	<para>Initializes an instance of the <see cref="StraighFlushHand"/> class.</para>
			/// </summary>
			/// <param name="flushOrStraightHand">
			///		The cards that create the straight flush.
			/// </param>
			/// <param name="family">
			///		The creating hand family
			/// </param>
            public StraighFlushHand(Hand flushOrStraightHand, StraightFlushFamily family)
                : base(family)
            {
                this.originalHand = flushOrStraightHand;
            }


			/// <summary>
			/// Returns an enumerator that iterates only through the cards of the hand.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate only through the cards of the hand.
			/// </returns>
            public override IEnumerator<Card> GetEnumerator()
            {
                return originalHand.GetEnumerator();
            }

			/// <summary>
			/// Gives a string representation of the hand.
			/// </summary>
			/// <returns>
			/// a String that represents this hand.
			/// </returns>
            public override string ToString()
            {
                return "Straight Flush";
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
