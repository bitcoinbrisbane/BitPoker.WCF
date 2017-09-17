using System;
using System.Collections.Generic;
using System.Text;

namespace BitPoker.Models.Deck
{
    /// <summary>
    /// The basic card structure
    /// </summary>
    [Serializable]
    public struct Card : IComparable<Card>
    {
        /// <summary>
        /// The number of cards in a suite
        /// </summary>
        public const int CARDS_IN_SUITE = 13;
        /// <summary>
        /// The number of suites
        /// </summary>
        public const int NUMBER_OF_SUITES = 4;
        /// <summary>
        /// The maximal ordinal value of the cards
        /// </summary>
        public const int MAX_ORDINAL_VALUE = CARDS_IN_SUITE * NUMBER_OF_SUITES;
        /// <summary>
        /// Defines an empty value of a card.
        /// </summary>
        public static readonly Card Empty;

        static Card()
        {
            Empty = new Card();
            Empty.cardValue = -1;
        }

        /// <summary>
        /// Creates a new card structure
        /// </summary>
        /// <param name="value">The card value, must be in the range [0-<see cref="CARDS_IN_SUITE"/>)</param>
        /// <param name="suite">The card suite</param>
        /// <exception cref="ArgumentOutOfRangeException">Is thrown if the <paramref name="value"/> is not in the defined range.</exception>
        public Card(int value, Suite suite)
        {
            if (value < 0 || value >= CARDS_IN_SUITE)
                throw new ArgumentOutOfRangeException("value", "value must be in the range [0-" + (CARDS_IN_SUITE - 1) + "]");
            this.cardValue = value;
            this.cardSuite = suite;
        }

        /// <summary>
        /// A helper method which generates the string description of a card
        /// </summary>
        /// <param name="cardValue">The card value</param>
        /// <param name="cardSuite">The card suite</param>
        /// <returns>A string describing the card</returns>
        private static string generateStringValue(int cardValue, Suite cardSuite)
        {
            if (cardValue > -1 && cardValue < CARDS_IN_SUITE)
            {
                string name = string.Empty;
                // recall that the cards are sorted by their game value.
                if (cardValue < 9 && cardValue > -1)
                    name = "" + (cardValue + 2);
                else
                {
                    switch (cardValue)
                    {
                        case 9: name = "J"; break;
                        case 10: name = "Q"; break;
                        case 11: name = "K"; break;
                        case 12: name = "A"; break;
                    }
                }
                name = name.PadLeft(2, ' ');
                char symbol = ' ';
                // use unicode suite symbols
                switch (cardSuite)
                {
                    case Suite.Spades: symbol = (char)0x2660; break; 
                    case Suite.Clubs: symbol = (char)0x2663; break;
                    case Suite.Hearts: symbol = (char)0x2665; break;
                    case Suite.Diamonds: symbol = (char)0x2666; break;
                }

                return string.Format("{0}{1}", name, symbol);
            }
            else if (cardValue == -1) return "Empty";
            else return "<unknown>";
        }

        private int cardValue;
        /// <summary>
        /// Gets the card value.
        /// </summary>
        /// <remarks>
        /// The card value is defined as the game value. The value must be in the range [0-<see cref="CARDS_IN_SUITE"/>). <br/>
        /// The card game value is sorted from high to low where 2 has the lowest value (0) and Ace has the highest value (12). <br/>
        /// If the card is <see cref="Empty"/> then the value is -1.
        /// </remarks>
        public int CardValue
        {
            get { return cardValue; }
        }

        private Suite cardSuite;
        /// <summary>
        /// Gets the card suite.
        /// </summary>
        public Suite CardSuite
        {
            get { return cardSuite; }
        }

        /// <summary>
        /// Determines whether the specified Object is equal to the current Object
        /// </summary>
        /// <param name="obj">The Object to compare with the current Object.</param>
        /// <returns>
        /// True if the other object is a card structure with the same <see cref="OrdinalValue"/>
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            Card other = (Card)obj;
            // cards are equal if they represent the same value
            return this.OrdinalValue == other.OrdinalValue;
        }

        /// <summary>
        /// Determines whether the specified card is equal to the current card
        /// </summary>
        /// <param name="other">The card to compare with the current card.</param>
        /// <returns>
        /// True if the other card has the same <see cref="OrdinalValue"/>
        /// </returns>
        public bool Equals(Card other)
        {
            return this.OrdinalValue == other.OrdinalValue;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current card
        /// </returns>
        public override int GetHashCode()
        {
            return OrdinalValue;
        }

        /// <summary>
        /// Determines whether two specified Cards have the same value
        /// </summary>
        /// <param name="left">A card value</param>
        /// <param name="right">Another card value</param>
        /// <returns>
        /// True if the cards represent the same card value.
        /// </returns>
        public static bool operator ==(Card left, Card right)
        {
            return left.Equals(right);
        }
        /// <summary>
        /// Determines whether two specified Cards doesn't have the same value
        /// </summary>
        /// <param name="left">A card value</param>
        /// <param name="right">Another card value</param>
        /// <returns>
        /// True if the cards doesn't represent the same card value.
        /// </returns>
        public static bool operator !=(Card left, Card right)
        {
            return !left.Equals(right);
        }
        /// <summary>
        /// Determines whether the left card is smaller than the right card
        /// </summary>
        /// <param name="left">A card value</param>
        /// <param name="right">Another card value</param>
        /// <returns>True if the left card has a smaller card value than the right card</returns>
        /// <remarks>
        /// See <see cref="CompareTo"/> regarding cards comparision.
        /// </remarks>
        public static bool operator <(Card left, Card right)
        {
            return (left.CompareTo(right) < 0);
        }

        /// <summary>
        /// Determines whether the left card is larger than the right card
        /// </summary>
        /// <param name="left">A card value</param>
        /// <param name="right">Another card value</param>
        /// <returns>True if the left card has a larger card value than the right card</returns>
        /// <remarks>
        /// See <see cref="CompareTo"/> regarding cards comparision.
        /// </remarks>
        public static bool operator >(Card left, Card right)
        {
            return (left.CompareTo(right) > 0);
        }

        /// <summary>
        /// Gets the card ordinal value. The card ordinal value is unique and composed of both the 
        /// <see cref="CardValue"/> and the <see cref="CardSuite"/>
        /// </summary>
        public int OrdinalValue
        {
            get { return GetOridnalValue(this); }
        }

        /// <summary>
        /// Gets the card ordinal value. The card ordinal value is unique and composed of both the 
        /// <see cref="CardValue"/> and the <see cref="CardSuite"/>
        /// </summary>
        /// <param name="card">The card whose card ordinal value is returned</param>
        public static int GetOridnalValue(Card card)
        {
            return card.cardValue + (int)card.cardSuite;
        }

        /// <summary>
        /// Gets a card value from an ordinal value.
        /// </summary>
        /// <param name="value">The value (must be in the range [0-<see cref="MAX_ORDINAL_VALUE"/>)</param>
        /// <returns>
        /// The card value of the card in the given value position.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Is thrown is the <paramref name="value"/> is not in the defined range</exception>
        public static Card FromOrdinalValue(int value)
        {
            if (value < 0 || value >= MAX_ORDINAL_VALUE)
                throw new ArgumentOutOfRangeException("value", "value must be in the range [0-" + (MAX_ORDINAL_VALUE -1)  + "]");
            // get the card value in the suite
            int numericValue = value % CARDS_IN_SUITE;
            // get the card suite
            int suiteValue = value - numericValue;
            Suite suite = (Suite)suiteValue;
            // return a new structure
            return new Card(numericValue, suite);
        }

        #region IComparable<Card> Members
        /// <summary>
        /// Compares the given card value to the current card value
        /// </summary>
        /// <param name="other">
        /// The other card value to compare with.
        /// </param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings:
        /// <list type="bullet">
        /// <listheader>
        /// <term>Value</term><description>Meaning</description>
        /// </listheader>
        /// <item>
        /// <term>Less than zero</term><description>This value is less than <paramref name="other"/>. </description>
        /// </item>
        /// <item>
        /// <term>Zero</term><description>This value equals to <paramref name="other"/>. </description>
        /// </item>
        /// <item>
        /// <term>Greater than zero</term><description>This value is greater than <paramref name="other"/>. </description>
        /// </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Cards are compared only using their <see cref="CardValue"/>, there is no defined order of the card <see cref="CardSuite"/>.<br/>
        /// <note>Use <see cref="Equals(Card)"/> to determine if both cards represents the same card. Don't rely on this method 0 return value</note>
        /// </remarks>
        public int CompareTo(Card other)
        {
            // cards are ordered only by their value, the suite does not define order.
            return cardValue - other.cardValue;
        }

        #endregion
        /// <summary>
        /// Determines if the given card has the same suite as the current card
        /// </summary>
        /// <param name="other">Another card value</param>
        /// <returns>
        /// True if both cards has the same <see cref="CardSuite"/>
        /// </returns>
        public bool AreOfSameSuite(Card other)
        {
            return cardSuite == other.cardSuite;
        }
        /// <summary>
        /// Determines if the given card has the same numeric value as the current card
        /// </summary>
        /// <param name="other">Another card value</param>
        /// <returns>
        /// True if both cards has the same <see cref="CardValue"/>
        /// </returns>
        public bool AreOfSameNumericValue(Card other)
        {
            return cardValue == other.cardValue;
        }
        /// <summary>
        /// Determines if the given card has an adjacent numeric value to the current card
        /// </summary>
        /// <param name="other">Another card value</param>
        /// <returns>
        /// True if the given card has an adjacent card value to this card.
        /// </returns>
        /// <remarks>
        /// <note>The as defined by the game value, Ace and 2 are adjacent.</note>
        /// </remarks>
        public bool AreNumericValuesAdjacent(Card other)
        {
            int diff = Math.Abs(cardValue - other.cardValue);

            return (diff == 1 || diff == CARDS_IN_SUITE - 1);
        }

        /// <summary>
        /// Returns a String that represents the current card.
        /// </summary>
        /// <returns>
        /// A String that represents the current card.
        /// </returns>
        public override string ToString()
        {
            return generateStringValue(cardValue, cardSuite);
        }
    }
}
