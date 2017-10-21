using System;
using System.Collections.Generic;
using BitPoker.Models.Deck;

namespace BitPoker.Models.Hands
{
	/// <summary>
	/// Rpresents a certain poker hand.
	/// </summary>
    public abstract class Hand : IComparable<Hand>, IEnumerable<Card>
    {
        private HandFamily creatingFamily;

		/// <summary>
		/// 	<para>Initializes an instance of the <see cref="Hand"/> class.</para>
		/// </summary>
		/// <param name="family">
		///		The family that created the current hand.
		/// </param>
        protected Hand(HandFamily family)
        {
            this.creatingFamily = family;
        }

		/// <summary>
		/// The family that created this hand.
		/// </summary>
        public HandFamily Family
        {
            get { return creatingFamily; }
        }

        #region IComparable<Hand> Members

		/// <summary>
		/// Compares the current hand to another hand.
		/// The main purpose is to check which hand is stronger.
		/// </summary>
		/// <param name="other">The hand to compare to.</param>
		/// <returns>
		///		A 32-bit signed integer that indicates the relative order of the hands <br/>
		///		being compared. The return value has the following meanings: Value Meaning <br/>
		///		Less than zero This object is less than the other parameter.Zero This object <br/>
		///		is equal to other. Greater than zero This object is greater than other.
		/// </returns>
		/// <exception cref="System.InvalidOperationException">
		///		If the hands have the same family but different types of hands.
		/// </exception>
        public int CompareTo(Hand other)
        {
			// Try and compare using the hand families
            int familyValue = this.creatingFamily.CompareTo(other.creatingFamily);
            if (familyValue == 0)
            {
                if (other.GetType() != this.GetType())
                    throw new InvalidOperationException("Same family created different typed hands...");
                return CompareOverride(other); // If unsuccessful, use another method
            }
            else
                return familyValue;
        }

		/// <summary>
		/// Provides a way to compare hands, that is not by their families.
		/// The default way is to compare the hands card by card.
		/// </summary>
		/// <param name="other">The hand to compare to.</param>
		/// <returns>
		///		A 32-bit signed integer that indicates the relative order of the hands <br/>
		///		being compared. The return value has the following meanings: Value Meaning <br/>
		///		Less than zero This object is less than the other parameter.Zero This object <br/>
		///		is equal to other. Greater than zero This object is greater than other.
		/// </returns>
        protected virtual int CompareOverride(Hand other)
        {
            return CompareOneByOne(other);
        }

		/// <summary>
		/// Compares the current hand to another hand, by going over each card and comparing them.
		/// </summary>
		/// <param name="other">The hand to compare to.</param>
		/// <returns>
		///		A 32-bit signed integer that indicates the relative order of the hands <br/>
		///		being compared. The return value has the following meanings: Value Meaning <br/>
		///		Less than zero This object is less than the other parameter.Zero This object <br/>
		///		is equal to other. Greater than zero This object is greater than other.
		/// </returns>
		/// <exception cref="System.InvalidOperationException">
		///		If the hands have different number of cards in them.
		/// </exception>
        protected int CompareOneByOne(Hand other)
        {
            IEnumerator<Card> thisEnumerator = GetAllCards();
            IEnumerator<Card> otherEnumerator = other.GetAllCards();

            while (thisEnumerator.MoveNext() && otherEnumerator.MoveNext())
            {
                if (!thisEnumerator.Current.AreOfSameNumericValue(otherEnumerator.Current))
                    return thisEnumerator.Current.CompareTo(otherEnumerator.Current);
            }

            if (thisEnumerator.MoveNext() ^ otherEnumerator.MoveNext())
                throw new InvalidOperationException("Hands have different amount of cards to compare");
            
            return 0;

        }

        #endregion

        #region IEnumerable<Card> Members

		/// <summary>
		/// Returns an enumerator that iterates only through the cards of the hand.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate only through the cards of the hand.
		/// </returns>
        public abstract IEnumerator<Card> GetEnumerator();

		/// <summary>
		/// Returns an enumerator that iterates through all the player's cards, not just the hand cards.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate only through all the player's cards.
		/// </returns>
        public abstract IEnumerator<Card> GetAllCards();

        #endregion

        #region IEnumerable Members

		/// <summary>
		/// Returns an enumerator that iterates only through the cards of the hand.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate only through the cards of the hand.
		/// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

		/// <summary>
		/// Gives a string representation of the hand.
		/// </summary>
		/// <returns>
		/// a String that represents this hand.
		/// </returns>
        public abstract override string ToString();
    }
}
