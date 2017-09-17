using System;
using System.Collections.Generic;
using System.Text;
using PokerRules.Games;
using PokerEngine.Betting;
using BitPoker.Models.Deck;
using BitPoker.Models.Hands;
using System.Linq;

namespace PokerEngine.Engine
{
    /// <summary>
    /// The basic engine which manages five card draw games.
    /// </summary>
    /// <remarks>
    /// In five card draw, each player is dealt 5 cards, and can draw at most 3 cards.
    /// </remarks>
    public abstract class BaseFiveCardEngine : BaseEngine
    {
        // The game instance will be initialized in GetNewGame
        private FiveCardDrawGame game;
        
        /// <summary>
        /// Called by the engine in the begining of each round.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="FiveCardDrawGame"/> game
        /// </returns>
        protected override BaseGame GetNewGame()
        {
            game =  new FiveCardDrawGame();
            return game;
        }

        /// <summary>
        /// Implements the Five Card Draw game logic.
        /// </summary>
        /// <remarks>
        /// In five card draw, each player is dealt 5 cards, a betting round is performed then
        /// each player can draw at most 3 cards. The game ends after a final betting round.
        /// </remarks>
        protected override void OnPlayNextRound()
        {
            // if there are any round players, continue with the draw
            if (HasRoundPlayers) 
            {
                // manage player draws
                handleDraws();
                // perform final betting round
                handleLastRaise();
            }

        }


        /// <summary>
        /// Called to manage players draw.
        /// </summary>
        private void handleDraws()
        {
            // update the player cards
            WaitSynchronizePlayers();
            // go over all of the playing players
            foreach (Player curPlayer in PlayingPlayers)
            {
                // Get the player index which identifies the player in the game
                int playerIndex = GetPlayerGameIndex(curPlayer);
                // Create a new drawing action for the player
                PlayerDrawingAction action = new PlayerDrawingAction();
                // Call derived class with betting action and wait for a response
                WaitPlayerDrawingAction(curPlayer, action);
                // check that there are any cards to draw.
                if (action.DrawnCards.Count > 0)
                {
                    // Get the first 3 distinct cards
                    var drawnCards = action.DrawnCards.Distinct().Take(3);
                    // Draw the cards on behalf of the player
                    game.Draw(playerIndex, drawnCards.ToArray());
                    // update the player container:
                    // first remove the old cards,
                    foreach (Card oldCard in drawnCards)
                    {
                        curPlayer.Cards.Remove(oldCard);
                    }
                    // then add the new cards
                    foreach (Card newCard in game.GetPlayerCards(playerIndex))
                    {
                        if (!curPlayer.Cards.Contains(newCard))
                        {
                            curPlayer.Cards.Add(newCard);
                        }
                    }
                    // Notify the derived classes with the player and the new cards.
                    NotifyPlayerNewCards(curPlayer);
                }
            }
            // Notify derived class that there are no more draws.
            NotifyDrawComplete();
        }

        /// <summary>
        /// Called by the engine to notify the player with the new cards
        /// </summary>
        /// <param name="curPlayer">The player with the new drawn cards</param>
        protected virtual void NotifyPlayerNewCards(Player curPlayer)
        {
        }

        /// <summary>
        /// Called by the engine after the drawing round completes.
        /// </summary>
        protected virtual void NotifyDrawComplete()
        {

        }

        /// <summary>
        /// A method which is used to get the player drawing action. Derived classes must override this method to perform the action.
        /// </summary>
        /// <param name="player">The player which may draw new cards</param>
        /// <param name="action">The drawing action which must be updated with the player drawing selection</param>
        protected abstract void WaitPlayerDrawingAction(Player player, PlayerDrawingAction action);

        /// <summary>
        /// Perform another drawing round.
        /// </summary>
        private void handleLastRaise()
        {
            // Start raising
            DoRaiseRound();
            // notify that the raise round completed.
            NotifyRaiseComplete(false);
        }

    }
}
