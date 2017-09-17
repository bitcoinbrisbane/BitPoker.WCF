using System;
using System.Collections.Generic;
using PokerRules.Games;
using BitPoker.Models.Deck;
using PokerEngine;
using PokerEngine.Engine;
namespace PokerService
{
    /// <summary>
    /// This interface defines a helping class which can be composed inside a <see cref="BaseEngine"/> and 
    /// provide common user input/output operations.
    /// </summary>
    public interface IEngineHelper
    {
        /// <summary>
        /// Called when the current hand is raised. 
        /// </summary>
        /// <param name="currentHand">
        /// The current hand which is played
        /// </param>
        void NotifyCurrentHand(int currentHand);

        /// <summary>
        /// Called when the game round starts.
        /// </summary>
        /// <param name="potAmount">The initial pot amount</param>
        /// <param name="dealer">The current round dealer</param>
        void NotifyAntesAndDealer(int potAmount, Player dealer);

        /// <summary>
        /// Called when the game round starts and the blind open is made
        /// </summary>
        /// <param name="opener">The blind opener</param>
        /// <param name="openAmount">The open amount, can be 0</param>
        void NotifyBlindOpen(Player opener, int openAmount);

        /// <summary>
        /// Called when the game round starts and the blind raise is made
        /// </summary>
        /// <param name="raiser">The blind raiser</param>
        /// <param name="raiseAmount">The raise amount, can be 0</param>
        void NotifyBlindRaise(Player raiser, int raiseAmount);

        /// <summary>
        /// Called when a player loses the game. The player has lost all of the money and can't continue in the game.
        /// </summary>
        /// <param name="player">The losing player</param>
        void OnDeclareLoser(Player player);

        /// <summary>
        /// Called when a player wins a round. There can be more than one winner per round.
        /// </summary>
        /// <param name="player">The winning player</param>
        /// <param name="result">The winning player hand</param>
        void OnDeclareWinner(Player player, GameResult result);

        /// <summary>
        /// Called by the engine when the round is over. Override this method to determine if another round will be played.
        /// </summary>
        /// <returns>
        /// True - to indicate another round will be played, False - to finish the game.
        /// </returns>
        /// <remarks>
        /// On each round end, either this method or <see cref="WaitGameOver"/> will be called.
        /// </remarks>
        bool WaitForAnotherRound();

        /// <summary>
        /// Called by the engine to get the players for the game. Derived classes must override it and return a collection 
        /// of players.
        /// </summary>
        /// <returns>
        /// A non null enumerable of players (must have at least 2 players)
        /// </returns>
        IEnumerable<Player> WaitForPlayers();

        /// <summary>
        /// Called by the engine when the game is over and there is a single player standing.
        /// Override this method to add logic for game over.
        /// </summary>
        /// <param name="winner">
        /// The winnig player
        /// </param>
        /// <remarks>
        /// On each round end, either this method or <see cref="WaitForAnotherRound"/> will be called.
        /// </remarks>
        void WaitGameOver(Player winner);

        /// <summary>
        /// Called by the engine when a player needs to make a bet. 
        /// Derived classes must place player betting logic in overriden implementation.
        /// </summary>
        /// <param name="player">The current player which needs to bet</param>
        /// <param name="action">The player betting action which contains the current betting restrictions</param>
        /// <remarks>
        /// The action must be used to notify the engine of the player action. In the derived implementation take notice to
        /// the action <see cref="PlayerBettingAction.IsAllInMode"/>, <see cref="PlayerBettingAction.CanRaise"/> and 
        /// <see cref="PlayerBettingAction.RaiseAmount"/> (which holds the minimal raise amount at first)
        /// </remarks>
        void WaitPlayerBettingAction(Player player, PlayerBettingAction action);

        /// <summary>
        /// Called by the engine in various occasions to update the players information. 
        /// </summary>
        /// <param name="orderedPlayers">The players in their current round order</param>
        /// <param name="potAmount">The current pot amount</param>
        /// <param name="communityCards">The game community cards, if any. Can be null</param>
        void WaitSynchronizePlayers(IEnumerable<Player> orderedPlayers, int potAmount, Card[] communityCards);

        /// <summary>
        /// Called when the class initializes by <see cref="BaseEngine.Initialize"/>. Override it to add post constructor logic.
        /// </summary>
        void OnInitialize();

        /// <summary>
        /// Called when the game run has started. Override it to add logic of game initialization
        /// </summary>
        void OnRunStarted();

        /// <summary>
        /// Called after a betting round has completed
        /// </summary>
        /// <param name="allFolded">A flag indicating if all of the players folded and there is a single winner</param>
        void NotifyRaiseComplete(bool allFolded);

        /// <summary>
        /// Called after each round with a queue of losing players, the queue can be modified.
        /// </summary>
        /// <param name="losers">The queue of losers.</param>
        /// <remarks>
        /// Derived classes may modify the queue to add/remove losers.
        /// </remarks>
        void NotifyWinLoseComplete(Queue<Player> losers);

        /// <summary>
        /// Called at the round end to allow dervied classes to add new players. 
        /// </summary>
        /// <param name="toAdd">The queue of players to add to new players.</param>
        /// <remarks>
        /// This method called only when the game isn't in a <see cref="BaseEngine.TournamentMode"/>
        /// <note>Note that the newly added players must have their initial money initialized to a positive sum.</note>
        /// </remarks>
        void GetNewPlayersAtRoundEnd(Queue<Player> toAdd);

        /// <summary>
        /// Called when the game run has completed. Override it to add cleanup logic
        /// </summary>
        void OnRunComplete();
       
        /// <summary>
        /// Get a safe copy of the player.
        /// </summary>
        /// <param name="player">A non null value of a player</param>
        /// <returns>
        /// A copy of the player which is safe to send to other players.
        /// </returns>
        /// <remarks>
        /// Since the point of poker is hiding your hand from the other players, this method creates a copy which can be sent
        /// to other players. (It may remove/set as empty, some or all of the player cards)
        /// </remarks>
        Player GetSafePlayer(Player player);



        /// <summary>
        /// Called after each player performs an action
        /// </summary>
        /// <param name="player">The player which performed the action </param>
        /// <param name="betAction">The action performed</param>
        /// <param name="callAmount">The amount which the player had to call</param>
        /// <param name="raiseAmount">The amount which the player raised (if any)</param>
        void NotifyPlayerAction(Player player, BetAction betAction, int callAmount, int raiseAmount);

        /// <summary>
        /// Gets or sets the safe player callback. The callback must not be null.
        /// </summary>
        /// <remarks>
        /// Use this callback to change the way a <see cref="GetSafePlayer"/> returns a player. 
        /// The callback first argument is the safe copy which needs to be updated, the second argument is the original player.
        /// </remarks>
        Action<Player, Player> SetSafePlayerCards { get; set;}

    }
}
