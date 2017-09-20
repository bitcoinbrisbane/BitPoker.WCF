using System;
using System.Collections.Generic;
using System.Text;
using BitPoker.Models.Deck;

namespace BitPoker.Models.Hands
{
	/// <summary>
	/// a Helper class for creating a N of a Kind hands.
	/// <remarks>N of a Kind hands are hands in which there are N cards of the same type.</remarks>
	/// </summary>
    internal static class NofAKindFamily
    {
		/// <summary>
		/// Tries to find a specific amount of cards that are the same in the provided cards.
		/// </summary>
		/// <param name="cards">The cards to search in.</param>
		/// <param name="numberOfMatches">The number of cards that should be the same.</param>
		/// <returns>The highest card valued match in the cards, null if the operation failed</returns>
        internal static List<Card> GetHighestCards(IList<Card> cards, int numberOfMatches)
        {

            List<Card> result = null;
            if (cards.Count >= numberOfMatches) // check if possible
            {
				
                Dictionary<int, List<Card>> numericValueToCards = new Dictionary<int, List<Card>>();

                foreach (Card card in cards) // try to find a number of matches to every card
                {
                    if (!numericValueToCards.ContainsKey(card.CardValue)) // create a new search for the card
                    {
                        numericValueToCards.Add(card.CardValue, new List<Card>());
                    }

                    List<Card> curList = numericValueToCards[card.CardValue];
                    if (curList.Count < numberOfMatches)
                    {
                        curList.Add(card); // add to found matches

                        if (curList.Count == numberOfMatches) // found secific amount
                        {
                            if (result == null)
                                result = curList;
                            else
                            {
                                if (result[0] < curList[0]) // keep the highest result
                                    result = curList;
                            }

                        }

                    }
                }
            }
            return result;
        }

        private static HighCardFamily highCardHelper = new HighCardFamily();

		/// <summary>
		/// Returns the cards that are left, in order, after removing the same value cards from the original cards.
		/// </summary>
		/// <param name="originalCards">The original cards.</param>
		/// <param name="someOfAKind">The cards that are of the same value.</param>
		/// <returns>The cards that remain after the removal of the provided same value cards.</returns>
        internal static IEnumerable<Card> GetHighCards(IList<Card> originalCards, List<Card> someOfAKind)
        {
            List<Card> clone = new List<Card>();
            clone.AddRange(originalCards);

            HandFamily.RemoveCardValue(clone, someOfAKind[0]);

            Hand bestHighCard = highCardHelper.GetBestHand(clone);
            int completionCount = 5 - someOfAKind.Count;
            if (bestHighCard == null)
            {
                Card[] result = new Card[completionCount];
                for (int i = 0; i < result.Length; ++i)
                {
                    result[i] = Card.Empty;
                }
                return result;
            }
            else
                return HighCardFamily.GetHighCards(bestHighCard, completionCount);

        }
    }
}
