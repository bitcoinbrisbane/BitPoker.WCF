using System;
using System.Collections.Generic;
using System.Text;
using BitPoker.Models.Deck;

namespace BitPoker.Models.Hands
{
	/// <summary>
	/// Represents a family/type of poker hand.<br/>
	/// Mainly used to create <see cref="PokerRules.Hands.Hand"/> instances from a collection of cards, <br/>
	/// and for comparing purposes.
	/// </summary>
    public abstract class HandFamily : IComparable<HandFamily>
    {
		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="HandFamily"/> class.</para>
		/// </summary>
		/// <param name="familyValue">
		///		a Numeric value for the family, to represent its stregth in the current poker rules.
		/// </param>
        protected HandFamily(int familyValue)
        {
            this.familyValue = familyValue;
        }

		/// <summary>
		/// Tries a <see cref="PokerRules.Hands.Hand"/> of the family's type from the given cards.
		/// </summary>
		/// <param name="cards">The cards from which to create the hand.</param>
		/// <returns>
		///	a <see cref="PokerRules.Hands.Hand"/> of the family's type, or null if the provided cards <br/>
		///	do not contain the specific hand.
		/// </returns>
        public Hand GetBestHand(IList<Card> cards)
        {
            List<Card> clone = new List<Card>();
            foreach (Card card in cards)
            {
                if (card != Card.Empty)
                    clone.Add(card);
            }
            return GetBestHandOverride(clone);
        }

		/// <summary>
		/// Tries to create a <see cref="PokerRules.Hands.Hand"/> of the family's type from the given cards.
		/// </summary>
		/// <param name="cards">The cards from which to create the hand.</param>
		/// <returns>
		///	a <see cref="PokerRules.Hands.Hand"/> of the family's type, or null if the cards provided <br/>
		///	do not contain the specific hand.
		/// </returns>
        protected abstract Hand GetBestHandOverride(List<Card> cards);

        private int familyValue;

		/// <summary>
		///	a Numeric value for the family, to represent its stregth in the current poker rules.
		/// </summary>
        public int FamilyValue { get { return familyValue; } }

		/// <summary>
		/// Removes all cards with the same card value from the given collection of cards.
		/// </summary>
		/// <param name="cards">The collection from which to remove the cards.</param>
		/// <param name="valueToRemove">The value of the card to remove.</param>
        protected internal static void RemoveCardValue(List<Card> cards, Card valueToRemove)
        {
            cards.RemoveAll(
                delegate(Card cur)
                {
                    return cur.AreOfSameNumericValue(valueToRemove);
                }
            );
        }

        #region IComparable<HandFamily> Members

		/// <summary>
		/// Compares the current hand family to another hand.
		/// The main purpose is to check which hand family is stronger.
		/// </summary>
		/// <param name="other">The hand family to compare to.</param>
		/// <returns>
		///		A 32-bit signed integer that indicates the relative order of the hand families <br/>
		///		being compared. The return value has the following meanings: Value Meaning <br/>
		///		Less than zero This object is less than the other parameter.Zero This object <br/>
		///		is equal to other. Greater than zero This object is greater than other.
		/// </returns>
        public int CompareTo(HandFamily other)
        {
            return FamilyValue - other.FamilyValue;
        }

        #endregion

    }
}
