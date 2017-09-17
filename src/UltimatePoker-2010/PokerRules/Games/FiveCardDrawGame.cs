using System;
using System.Collections.Generic;
using BitPoker.Models.Deck;
using System.Collections.ObjectModel;

namespace PokerRules.Games
{
    /// <summary>
    /// The game class for Five Card Draw
    /// </summary>
    /// <remarks>
    /// In five card draw, each player is dealt 5 cards, and can draw at most 3 cards.
    /// </remarks>
    public class FiveCardDrawGame : BaseGame
    {
        /// <summary>
        /// Gets the maximal player count
        /// </summary>
        public override int MaximalPlayersLimit
        {
            // 5 cards + 3 possible draws
            // 52 /(5+3) = 6.5
            get { return 6; }
        }

        /// <summary>
        /// Called when the game starts, deals all of the player 5 cards.
        /// </summary>
        protected override void OnBeginGame()
        {
            DealToAll(5);
        }

        /// <summary>
        /// Draws the given player the specified cards.
        /// </summary>
        /// <param name="player">The player of which to draw cards for</param>
        /// <param name="cards">The cards to draw, can be empty or null, but can't be over 3 cards.</param>
        /// <exception cref="ArgumentOutOfRangeException">Is thrown if there are more than 3 cards to draw.</exception>
        /// <exception cref="IndexOutOfRangeException">Is thrown if the given player is out of the range [0-<see cref="BaseGame.NumberOfPlayers"/>)</exception>
        /// <exception cref="InvalidOperationException">Is thrown if the old card did not exist in the player hand</exception>
        /// <remarks>
        /// Each drawn card is replaced by a unique card out of the deck.
        /// </remarks>
        public void Draw(int player, Card[] cards)
        {
            // no need to draw anything
            if (cards == null || cards.Length == 0)
                return;
            // can't draw more than 3 cards
            if (cards.Length > 3)
                throw new ArgumentOutOfRangeException("cards", "can draw at most 3 cards");
            // replace each given card with the next card out of the top of the deck
            for (int i = 0; i < cards.Length; ++i)
            {
                ReplaceCard(player, cards[i], Deck.Deal());
            }
        }

        /// <summary>
        /// Called when the game ends, does nothing
        /// </summary>
        protected override void OnEndGame()
        {

        }
    }
}
