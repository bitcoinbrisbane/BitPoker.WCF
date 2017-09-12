using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerEngine.Engine;
using PokerEngine;
using PokerRules.Deck;
using PokerRules.Games;

namespace PokerService
{
    /// <summary>
    /// A helper based implementation of the Omaha Hold'em server
    /// </summary>
    public class OmahaHoldemServer : BaseOmahaHoldemEngine
    {
        // the helper is used to communicate and run the game.
        private IEngineHelper helper;

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="OmahaHoldemServer"/> class.</para>
        /// </summary>
        /// <param name="helper">The helper which provides the input/output operations
        /// </param>
        public OmahaHoldemServer(IEngineHelper helper)
        {
            this.helper = helper;
        }

       
        /// <summary>
        /// Called by the engine to get the players for the game. Derived classes must override it and return a collection 
        /// of players.
        /// </summary>
        /// <returns>
        /// A non null enumerable of players (must have at least 2 players)
        /// </returns>
        protected override IEnumerable<Player> WaitForPlayers()
        {
            return helper.WaitForPlayers();
        }

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
        protected override void WaitGameOver(Player winner)
        {
            helper.WaitGameOver(winner);
        }

        /// <summary>
        /// Called when the current hand is raised. Default implementation will use <see cref="BaseEngine.AutoIncreaseOnHandDivider"/>
        /// to check automatic raise.
        /// </summary>
        /// <param name="currentHand">
        /// The current hand which is played
        /// </param>
        protected override void OnCurrentHandRaised(int currentHand)
        {
            base.OnCurrentHandRaised(currentHand);
            helper.NotifyCurrentHand(currentHand);
        }

        /// <summary>
        /// Called at the round end to allow dervied classes to add new players. 
        /// </summary>
        /// <param name="toAdd">The queue of players to add to new players.</param>
        /// <remarks>
        /// This method called only when the game isn't in a <see cref="BaseEngine.TournamentMode"/><note>Note that the newly added players must have their initial money initialized to a positive sum.</note></remarks>
        protected override void GetNewPlayersAtRoundEnd(Queue<Player> toAdd)
        {
            base.GetNewPlayersAtRoundEnd(toAdd);
            helper.GetNewPlayersAtRoundEnd(toAdd);
        }

        /// <summary>
        /// Called by the engine when the round is over. Override this method to determine if another round will be played.
        /// </summary>
        /// <returns>
        /// True - to indicate another round will be played, False - to finish the game.
        /// </returns>
        /// <remarks>
        /// On each round end, either this method or <see cref="WaitGameOver"/> will be called.
        /// </remarks>
        protected override bool WaitForAnotherRound()
        {
            return helper.WaitForAnotherRound();
        }

        /// <summary>
        /// Called by the engine in various occasions to update the players information. 
        /// </summary>
        /// <param name="orderedPlayers">The players in their current round order</param>
        /// <param name="potAmount">The current pot amount</param>
        /// <param name="communityCards">The community cards in the game</param>
        protected override void WaitSynchronizePlayers(IEnumerable<Player> orderedPlayers, int potAmount, Card[] communityCards)
        {
            helper.WaitSynchronizePlayers(orderedPlayers, potAmount, communityCards);
        }

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
        protected override void WaitPlayerBettingAction(Player player, PlayerBettingAction action)
        {
            helper.WaitPlayerBettingAction(player, action);
        }


        /// <summary>
        /// Called when the class initializes by <see cref="BaseEngine.Initialize"/>. Override it to add post constructor logic.
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            helper.OnInitialize();
        }

        /// <summary>
        /// Called when the game run has started. Override it to add logic of game initialization
        /// </summary>
        protected override void OnRunStarted()
        {
            base.OnRunStarted();
            helper.OnRunStarted();
        }

        /// <summary>
        /// Called when the game round starts.
        /// </summary>
        /// <param name="potAmount">The initial pot amount</param>
        /// <param name="dealer">The current round dealer</param>
        protected override void NotifyAntesAndDealer(int potAmount, Player dealer)
        {
            base.NotifyAntesAndDealer(potAmount, dealer);
            helper.NotifyAntesAndDealer(potAmount, dealer);
        }

        /// <summary>
        /// Called when the game round starts and the blind open is made
        /// </summary>
        /// <param name="opener">The blind opener</param>
        /// <param name="openAmount">The open amount, can be 0</param>
        protected override void NotifyBlindOpen(Player opener, int openAmount)
        {
            base.NotifyBlindOpen(opener, openAmount);
            helper.NotifyBlindOpen(opener, openAmount);
        }

        /// <summary>
        /// Called when the game round starts and the blind raise is made
        /// </summary>
        /// <param name="raiser">The blind raiser</param>
        /// <param name="raiseAmount">The raise amount, can be 0</param>
        /// <param name="openAmount">The original open amount which was notified by <see cref="NotifyBlindOpen"/></param>
        protected override void NotifyBlindRaise(Player raiser, int openAmount, int raiseAmount)
        {
            base.NotifyBlindRaise(raiser, openAmount, raiseAmount);
            helper.NotifyBlindRaise(raiser, raiseAmount);
        }

        /// <summary>
        /// Called after a betting round has completed
        /// </summary>
        /// <param name="allFolded">A flag indicating if all of the players folded and there is a single winner</param>
        protected override void NotifyRaiseComplete(bool allFolded)
        {
            base.NotifyRaiseComplete(allFolded);
            helper.NotifyRaiseComplete(allFolded);
        }

        /// <summary>
        /// Called after each round with a queue of losing players, the queue can be modified.
        /// </summary>
        /// <param name="losers">The queue of losers.</param>
        /// <remarks>
        /// Derived classes may modify the queue to add/remove losers.
        /// </remarks>
        protected override void NotifyWinLoseComplete(Queue<Player> losers)
        {
            base.NotifyWinLoseComplete(losers);
            helper.NotifyWinLoseComplete(losers);
        }

        /// <summary>
        /// Called after each player performs an action
        /// </summary>
        /// <param name="player">The player which performed the action </param>
        /// <param name="betAction">The action performed</param>
        /// <param name="callAmount">The amount which the player had to call</param>
        /// <param name="raiseAmount">The amount which the player raised (if any)</param>
        protected override void NotifyPlayerAction(Player player, BetAction betAction, int callAmount, int raiseAmount)
        {
            base.NotifyPlayerAction(player, betAction, callAmount, raiseAmount);
            helper.NotifyPlayerAction(player, betAction, callAmount, raiseAmount);
        }

        /// <summary>
        /// Called when a player loses the game. The player has lost all of the money and can't continue in the game.
        /// </summary>
        /// <param name="player">The losing player</param>
        /// <remarks>
        /// In a <see cref="BaseEngine.TournamentMode"/> the <see cref="BaseEngine.Ante"/> and the <see cref="BaseEngine.SmallRaise"/> are increased.
        /// </remarks>
        protected override void OnDeclareLoser(Player player)
        {
            base.OnDeclareLoser(player);
            helper.OnDeclareLoser(player);
        }

        /// <summary>
        /// Called when a player wins a round. There can be more than one winner per round.
        /// </summary>
        /// <param name="player">The winning player</param>
        /// <param name="result">The winning player hand</param>
        protected override void OnDeclareWinner(Player player, GameResult result)
        {
            base.OnDeclareWinner(player, result);
            helper.OnDeclareWinner(player, result);
        }

        /// <summary>
        /// Called when the game run has completed. Override it to add cleanup logic
        /// </summary>
        protected override void OnRunComplete()
        {
            base.OnRunComplete();
            helper.OnRunComplete();
        }


    }

}
