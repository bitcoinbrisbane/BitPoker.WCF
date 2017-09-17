using System;
using System.Collections.Generic;
using System.Text;
using PokerRules.Games;
using BitPoker.Models.Deck;
using System.Collections.ObjectModel;

namespace PokerEngine.Engine
{
    /// <summary>
    /// A base engine which implements the Texas Hold'em game logic.
    /// </summary>
    /// <remarks>
    /// Texas Hold'em is a game played with 7 cards.
    /// Each player is dealt 2 cards, these are called the Hole. They are unique to each player and can be seen only by the player.
    /// There are 5 community cards which are dealt in the following way:
    /// <list type="bullet">
    /// <item>3 cards at once, this is called the flop</item>
    /// <item>1 card, this is call the turn</item>
    /// <item>Last card, this is called the river</item>
    /// </list>
    /// The player can create the best hand using any combination of the Hole cards &amp; the Community cards.
    /// </remarks>
    public abstract class BaseTexasHoldemEngine : BaseEngine
    {
        // The game instance is used to perform game logic
        private TexasHoldem game;

        /// <summary>
        /// Called by the engine in the begining of each round.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="TexasHoldem"/> game
        /// </returns>
        protected override sealed BaseGame GetNewGame()
        {
            game = GetNewGameOverride();
            return game;
        }

        /// <summary>
        /// Called by the engine in the begining of each round.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="TexasHoldem"/> game
        /// </returns>
        /// <remarks>
        /// When deriving the class, you can't override <see cref="GetNewGame"/> so override this method if you need 
        /// to create another type of a <see cref="TexasHoldem"/>
        /// </remarks>
        protected virtual TexasHoldem GetNewGameOverride()
        {
            return new TexasHoldem();
        }

        /// <summary>
        /// Implements the texas hold'em logic.
        /// </summary>
        /// <remarks>
        /// Texas Hold'em is a game played with 7 cards.
        /// Each player is dealt 2 cards, these are called the Hole. They are unique to each player and can be seen only by the player.
        /// There are 5 community cards which are dealt in the following way:
        /// <list type="bullet">
        /// <item>3 cards at once, this is called the flop</item>
        /// <item>1 card, this is call the turn</item>
        /// <item>Last card, this is called the river</item>
        /// </list>
        /// The player can create the best hand using any combination of the Hole cards &amp; the Community cards.
        /// </remarks>
        protected override void OnPlayNextRound()
        {
            if (HasRoundPlayers) // check that there are any players left to play with.
            {
                // deal the flop
                game.Flop();
                // perform another betting round
                RaiseRound();

                // check that there are any more players
                if (HasRoundPlayers)
                {
                    // deal the turn
                    game.Turn();
                    // perform another betting round
                    RaiseRound();
                    // check that there are any more players
                    if (HasRoundPlayers)
                    {
                        // deal the river
                        game.River();
                        // perform the final betting round.
                        DoRaiseRound();
                        NotifyRaiseComplete(false);
                    }
                }

            }
        }

        /// <summary>
        /// Starts a raising round from the Dealer
        /// </summary>
        private void RaiseRound()
        {
            DoRaiseRound();
            NotifyRaiseComplete(!HasRoundPlayers);
        }

        /// <summary>
        /// Called by the engine in various occasions to update the players information. 
        /// </summary>
        /// <param name="orderedPlayers">The players in their current round order</param>
        /// <param name="potAmount">The current pot amount</param>
        /// <remarks>
        /// The texas hold'em engine, seals this and adds <see cref="WaitSynchronizePlayers(IEnumerable{Player},int,Card[])"/> instead.
        /// </remarks>
        protected override sealed void WaitSynchronizePlayers(IEnumerable<Player> orderedPlayers, int potAmount)
        {
            // create a copy of the community cards to pass to the derived classes
            Card[] communityCards = new Card[game.CommunityCards.Count];
            game.CommunityCards.CopyTo(communityCards, 0);
            // call the new overload:
            WaitSynchronizePlayers(orderedPlayers, potAmount, communityCards);
        }

        /// <summary>
        /// Called by the engine in various occasions to update the players information. 
        /// </summary>
        /// <param name="orderedPlayers">The players in their current round order</param>
        /// <param name="potAmount">The current pot amount</param>
        /// <param name="communityCards">The community cards in the game</param>
        protected abstract void WaitSynchronizePlayers(IEnumerable<Player> orderedPlayers, int potAmount, Card[] communityCards);

    }
}
