using System;
using System.Collections.Generic;
using BitPoker.Models.Deck;
using BitPoker.Models.Hands;

namespace PokerRules.Games
{
    /// <summary>
    /// A game result structure, it holds the end game values.
    /// </summary>
    public struct GameResult
    {
        /// <summary>
        /// Creates a new GameResult with the given player and the given best hand.
        /// </summary>
        /// <param name="player">The winning player</param>
        /// <param name="bestHand">The winning hand</param>
        public GameResult(int player, Hand bestHand)
        {
            this.player = player;
            this.bestHand = bestHand;
        }
        
        private int player;
        /// <summary>
        /// Gets the winning player
        /// </summary>
        public int Player
        {
            get { return player; }
        }

        private Hand bestHand;
        /// <summary>
        /// Gets the winning hand
        /// </summary>
        public Hand BestHand
        {
            get { return bestHand; }
        }
    }
}