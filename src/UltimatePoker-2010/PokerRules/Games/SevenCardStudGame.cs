using System;
using System.Collections.Generic;

namespace PokerRules.Games
{
    /// <summary>
    /// The game class for 7 Card stud
    /// </summary>
    /// <remarks>
    /// 7 Card stud is a game which is played with 7 cards. 
    /// The first, second and seventh card are visible only to the current player. The rest are visible to all.
    /// The player can create the best hand using all of the dealt cards.
    /// </remarks>
    public class SevenCardStudGame : BaseGame
    {
        // used to cound the number cards which can be dealt
        private int cardCount = 0;

        /// <summary>
        /// Gets the maximal player count
        /// </summary>
        public override int MaximalPlayersLimit
        {
            // 7 cards per player
            // 52 /7 = 7.42
            get { return 7; }
        }

        /// <summary>
        /// Called when the game starts, deals all of the player 3 cards.
        /// </summary>
        protected override void OnBeginGame()
        {
            DealToAll(3);
            cardCount = 3;
        }

        /// <summary>
        /// Deals each player the next card.
        /// </summary>
        /// <remarks>
        /// This method will have no effect after calling it 4 times. You can use <see cref="CanDeal"/> to check if 
        /// it will deal more cards
        /// </remarks>
        public void Deal()
        {
            if (CanDeal)
            {
                base.DealToAll(1);
                ++cardCount;
            }
        }

        /// <summary>
        /// Gets a value indicating if more cards can be dealt to the players
        /// </summary>
        public bool CanDeal
        {
            get
            {
                return cardCount < 7;
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
