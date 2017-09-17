using System;
using PokerEngine;
using PokerEngine.Engine;
using System.Collections.Generic;
using BitPoker.Models.Deck;
using PokerService;
namespace PokerConsole.Engine
{
    /// <summary>
    /// An interface which provides common client capabilities. Implementing classes will provide basic input/output capabilities.
    /// </summary>
    public interface IClientHelper
    {
        /// <summary>
        /// Called when the betting round finalizes in any case the round is over.
        /// </summary>
        void FinalizeBettingRound();

        /// <summary>
        /// Gets the login name of the client. This method may be called again after a call to <see cref="NotifyNameExists"/>
        /// </summary>
        /// <returns>A valid user name to login the client</returns>
        /// <remarks>
        /// This method must aquire the user login name. A new value must be returned after a call to <see cref="NotifyNameExists"/>
        /// </remarks>
        string GetName();

        /// <summary>
        /// Called when the betting round is completed and there is a single winner.
        /// </summary>
        void NotifyAllFolded();

        /// <summary>
        /// Called when a betting round is completed and all players have checked.
        /// </summary>
        void NotifyBetRoundComplete();

        /// <summary>
        /// Called before a betting round starts.
        /// </summary>
        void NotifyBetRoundStarted();

        /// <summary>
        /// Called by the client when the blind open is made.
        /// </summary>
        /// <param name="opened">The player which blind opens</param>
        /// <param name="openAmount">The player open amount, may be 0</param>
        void NotifyBlindOpen(Player opened, int openAmount);

        /// <summary>
        /// Called by the client when the blind raise is made.
        /// </summary>
        /// <param name="raiser">The player which blind raises</param>
        /// <param name="raiseAmount">The raise amound, may be 0</param>
        void NotifyBlindRaise(Player raiser, int raiseAmount);

        /// <summary>
        /// Called by the client to notify the current hand which is played
        /// </summary>
        /// <param name="currentHand">The current hand</param>
        void NotifyCurrentHand(int currentHand);

        /// <summary>
        /// Called by the client when a connection is made successfuly to the server
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint which was opened locally.
        /// </param>
        void NotifyConnectedToServer(System.Net.EndPoint endPoint);

        /// <summary>
        /// Called by the client when the connection to the server is closed
        /// </summary>
        void NotifyConnectionClosed();

        /// <summary>
        /// Called by the client when a round starts.
        /// </summary>
        /// <param name="dealer">The current round dealer</param>
        /// <param name="potAmount">The starting amount of money in the pot</param>
        void NotifyDealerAndPotAmount(Player dealer, int potAmount);

        /// <summary>
        /// Called by the client when a game is canceled by the server
        /// </summary>
        void NotifyGameCanceled();

        /// <summary>
        /// Called by the client when a game is already in progress. The server can't be connected
        /// </summary>
        void NotifyGameInProgress();

        /// <summary>
        /// Called by the client when the name returned by <see cref="GetName"/> already exists on the server.
        /// </summary>
        /// <remarks>
        /// Derived classes may use this method to create a new login name.
        /// </remarks>
        void NotifyNameExists();

        /// <summary>
        /// Called by the client when a new player is connected.
        /// </summary>
        /// <param name="player">The new player which was connected</param>
        void NotifyNewUserConnected(Player player);

        /// <summary>
        /// Called by the client when a player performs an action.
        /// </summary>
        /// <param name="player">The player which performed the action</param>
        /// <param name="betAction">The action performed</param>
        /// <param name="callAmount">The call amount if any, can be 0</param>
        /// <param name="raiseAmount">The raise amount if any, can be 0</param>
        void NotifyPlayerAction(Player player, BetAction betAction, int callAmount, int raiseAmount);

        /// <summary>
        /// Called when a player is thinking of a move
        /// </summary>
        /// <param name="thinkingPlayer">The player which is thinking</param>
        void NotifyPlayerIsThinking(Player thinkingPlayer);

        /// <summary>
        /// Called by the clien when a player wins/loses.
        /// </summary>
        /// <param name="player">The player which won/lost</param>
        /// <param name="isWinner">True - the player won. False - The player lost.
        /// </param>
        void NotifyPlayerStatus(Player player, bool isWinner);

        /// <summary>
        /// Called by the client after all of the results arrived
        /// </summary>
        void NotifyResultsIncomingCompleted();

        /// <summary>
        /// Called by the client before the results starts to arrive.
        /// </summary>
        void NotifyResultsIncomingStarted();

        /// <summary>
        /// Called by the client when a server identifies the running game.
        /// </summary>
        /// <param name="serverGame">The type of game the server is running</param>
        /// <param name="connectedPlayers">The number of players currently connected to the server</param>
        void NotifyRunningGame(ServerGame serverGame, int connectedPlayers);

        /// <summary>
        /// Called by the client when the current client can't participate in the current round.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifyStartingGame"/> is called prior to the round start.
        /// </remarks>
        void NotifySittingOut();

        /// <summary>
        /// Called by the client when the current round starts and the client is participating.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifySittingOut"/> is called prior to the round start.
        /// </remarks>
        void NotifyStartingGame();

        /// <summary>
        /// Called by the client when each round ends. 
        /// </summary>
        /// <param name="gameOver">A flag which indicates the game is over.</param>
        /// <param name="winner">The winning player (if any), may be null</param>
        void WaitOnRoundEnd(bool gameOver, Player winner);

        /// <summary>
        /// Called by the client to get the current player betting action. Derived classes must implement it and respond with a 
        /// proper action
        /// </summary>
        /// <param name="player">The player which needs to bet</param>
        /// <param name="action">The action to use to pass the player response</param>
        void WaitPlayerBettingAction(Player player, PlayerBettingAction action);

        /// <summary>
        /// Called by the client when an update message arrives.
        /// </summary>
        /// <param name="player">The players sorted by their round order</param>
        /// <param name="potInformation">The current state of the pot</param>
        /// <param name="communityCards">The community cards in the game (if any) may be null or in 0 length</param>
        void WaitSynchronization(IEnumerable<Player> player, PotInformation potInformation, Card[] communityCards);

        /// <summary>
        /// Called by the client when a player disconnects
        /// </summary>
        /// <param name="player">The player which was disconnected</param>
        void NotifyUserDisconnected(Player player);

        /// <summary>
        /// Called by the client to indicate that a player talked
        /// </summary>
        /// <param name="user">The talking user</param>
        /// <param name="message">The message which the user spoke</param>
        void UserTalked(Player user, string message);
    }
}
