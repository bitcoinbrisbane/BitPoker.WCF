using System;
using System.Collections.Generic;
using System.Text;
using BitPoker.Models.Deck;

namespace BitPoker.Models.Hands
{
	/// <summary>
	/// Represents a hand family of Flush hand.
	/// <remarks>a Flush hand, is a hand in which the highest combination is that all card are of the same suite.</remarks>
	/// </summary>
    public class FlushFamily : HandFamily
    {
		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="FlushFamily"/> class, with a zero family value.</para>
		/// </summary>
        public FlushFamily() : base(0) { }

		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="FlushFamily"/> class, with the given family value.</para>
		/// </summary>
        public FlushFamily(int familyValue) : base(familyValue) { }

		/// <summary>
		/// Tries to create a Flush <see cref="Hand"/> from the given cards.
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
            
			// init suites lists
            Suite[] suites = (Suite[])Enum.GetValues(typeof(Suite));
            SortedList<int, Card>[] suiteToCard = new SortedList<int, Card>[suites.Length];

            for (int i = 0; i < suiteToCard.Length; ++i)
            {
                suiteToCard[i] = new SortedList<int, Card>();
            }

			// add cards to appropriate suite lists
            List<SortedList<int, Card>> qualifiedLists = new List<SortedList<int, Card>>();
            foreach (Card card in cards)
            {
                int index = Array.IndexOf<Suite>(suites, card.CardSuite);
                SortedList<int, Card> curList = suiteToCard[index];
                if (!curList.ContainsKey(card.CardValue))
                {
                    curList.Add(card.CardValue, card);
                    if (curList.Count > 4)
                        qualifiedLists.Add(curList);
                }
            }
            if (qualifiedLists.Count == 0)
                return null;

            Queue<SortedList<int, Card>> toRemove = new Queue<SortedList<int, Card>>();

			// removes all low qualified lists, leaving the highest qualified flush
            for (int i = 1; i <= 5 && qualifiedLists.Count > 1; ++i)
            {
                toRemove.Clear();
                SortedList<int, Card> curHighList = qualifiedLists[0];
                Card curHighCard = qualifiedLists[0].Values[qualifiedLists[0].Count - i];
                foreach (SortedList<int, Card> curList in qualifiedLists)
                {
                    if (curList.Values[curList.Count - i] < curHighCard)
                        toRemove.Enqueue(curList);
                    else if (curList.Values[curList.Count - i] > curHighCard)
                    {
                        curHighCard = curList.Values[curList.Count - i];
                        toRemove.Enqueue(curHighList);
                        curHighList = curList;
                    }
                }

                while (toRemove.Count > 0)
                {
                    qualifiedLists.Remove(toRemove.Dequeue());
                }
            }

            return new FlushHand(qualifiedLists[0], this);
        }

		/// <summary>
		/// Represents a Flush hand.
		/// </summary>
        private class FlushHand : Hand
        {
            private List<Card> cards = new List<Card>();

			/// <summary>
			/// 	<para>Initializes an instance of the <see cref="FlushHand"/> class.</para>
			/// </summary>
			/// <param name="cards">
			///		The cards that create the flush.
			/// </param>
			/// <param name="family">
			///		The creating hand family.
			/// </param>
            public FlushHand(SortedList<int, Card> cards, FlushFamily family)
                : base(family)
            {
                for (int i = 1; i <= 5; ++i)
                    this.cards.Add(cards.Values[cards.Count - i]);
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
                return "Flush";
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
