using System;
using System.Collections.Generic;
using System.Text;
using PokerRules.Deck;

namespace PokerEngine.Engine
{
    /// <summary>
    /// A class which communicates the player drawing action
    /// </summary>
    [Serializable]
    public class PlayerDrawingAction
    {
        private List<Card> drawnCards = new List<Card>();
        /// <summary>
        /// Gets a list of cards which represents the player drawn cards
        /// </summary>
        /// <remarks>
        /// Note that the game restricts the amount of cards which can be drawn
        /// </remarks>
        public List<Card> DrawnCards
        {
            get { return drawnCards; }
        }

    }
}
