using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerRules.Games;

namespace PokerEngine.Engine
{
    /// <summary>
    /// The base engine which manages Seven Card Stud games.
    /// </summary>
    /// <remarks>
    /// 7 Card stud is a game which is played with 7 cards. 
    /// The first, second and seventh card are visible only to the current player. The rest are visible to all.
    /// The player can create the best hand using all of the dealt cards.
    /// There are betting rounds starting from the 3rd card and upto the final card dealt.
    /// </remarks>
    public abstract class BaseSevenCardStudEngine : BaseEngine
    {
        // The game instance which implements the game logic.
        private SevenCardStudGame game;

        /// <summary>
        /// Called by the engine in the begining of each round.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="SevenCardStudGame"/> game
        /// </returns>
        protected override sealed BaseGame GetNewGame()
        {
            this.game = GetNewGameOverride();
            return game;
        }

        /// <summary>
        /// Called by the engine in the begining of each round.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="SevenCardStudGame"/> game
        /// </returns>
        /// <remarks>
        /// When deriving the class, you can't override <see cref="GetNewGame"/> so override this method if you need 
        /// to create another type of a <see cref="SevenCardStudGame"/>
        /// </remarks>
        protected virtual SevenCardStudGame GetNewGameOverride()
        {
            return new SevenCardStudGame();
        }

        /// <summary>
        /// Implements the 7 card stud logic.
        /// </summary>
        /// <remarks>
        /// 7 Card stud is a game which is played with 7 cards. 
        /// The first, second and seventh card are visible only to the current player. The rest are visible to all.
        /// The player can create the best hand using all of the dealt cards.
        /// There are betting rounds starting from the 3rd card and upto the final card dealt.
        /// </remarks>
        protected override void OnPlayNextRound()
        {
            // as long as the game can deal cards & there are gamblers in the round, deal and perform a bet round.
            while (game.CanDeal && HasRoundPlayers)
            {
                game.Deal();

                RaiseRound();
            }
        }

        /// <summary>
        /// Performs another betting round, starting from the dealers
        /// </summary>
        private void RaiseRound()
        {
            DoRaiseRound();
            NotifyRaiseComplete(!HasRoundPlayers);
        }

    }
}
