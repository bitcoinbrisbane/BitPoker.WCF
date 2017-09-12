using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerConsole.Engine;
using PokerService;
using PokerRules.Deck;
using PokerEngine;

namespace PokerConsole.AI
{
    /// <summary>
    /// An empty implementation of the <see cref="IClientHelper"/>
    /// </summary>
    public class EmptyClientHelper : IClientHelper, IFiveCardClientHelper
    {
        #region IClientHelper Members

        /// <summary>
        /// Called when the betting round finalizes in any case the round is over.
        /// </summary>
        public void FinalizeBettingRound()
        {

        }

        /// <summary>
        /// Gets the login name of the client. This method may be called again after a call to <see cref="NotifyNameExists"/></summary>
        /// <returns>A valid user name to login the client</returns>
        /// <remarks>
        /// This method must aquire the user login name. A new value must be returned after a call to <see cref="NotifyNameExists"/></remarks>
        public string GetName()
        {
            throw new InvalidCastException();
        }

        /// <summary>
        /// Called when the betting round is completed and there is a single winner.
        /// </summary>
        public void NotifyAllFolded()
        {

        }

        /// <summary>
        /// Called when a betting round is completed and all players have checked.
        /// </summary>
        public void NotifyBetRoundComplete()
        {

        }

        /// <summary>
        /// Called before a betting round starts.
        /// </summary>
        public void NotifyBetRoundStarted()
        {

        }

        /// <summary>
        /// Called by the client when the blind open is made.
        /// </summary>
        /// <param name="opened">The player which blind opens</param>
        /// <param name="openAmount">The player open amount, may be 0</param>
        public void NotifyBlindOpen(PokerEngine.Player opened, int openAmount)
        {

        }

        /// <summary>
        /// Called by the client when the blind raise is made.
        /// </summary>
        /// <param name="raiser">The player which blind raises</param>
        /// <param name="raiseAmount">The raise amound, may be 0</param>
        public void NotifyBlindRaise(PokerEngine.Player raiser, int raiseAmount)
        {

        }

        /// <summary>
        /// Called by the client to notify the current hand which is played
        /// </summary>
        /// <param name="currentHand">The current hand</param>
        public void NotifyCurrentHand(int currentHand)
        {

        }

        /// <summary>
        /// Called by the client when a connection is made successfuly to the server
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint which was opened locally.
        /// </param>
        public void NotifyConnectedToServer(System.Net.EndPoint endPoint)
        {

        }

        /// <summary>
        /// Called by the client when a round starts.
        /// </summary>
        /// <param name="dealer">The current round dealer</param>
        /// <param name="potAmount">The starting amount of money in the pot</param>
        public void NotifyDealerAndPotAmount(PokerEngine.Player dealer, int potAmount)
        {

        }

        /// <summary>
        /// Called by the client when a game is canceled by the server
        /// </summary>
        public void NotifyGameCanceled()
        {

        }

        /// <summary>
        /// Called by the client when a game is already in progress. The server can't be connected
        /// </summary>
        public void NotifyGameInProgress()
        {

        }

        /// <summary>
        /// Called by the client when the name returned by <see cref="GetName"/> already exists on the server.
        /// </summary>
        /// <remarks>
        /// Derived classes may use this method to create a new login name.
        /// </remarks>
        public void NotifyNameExists()
        {

        }

        /// <summary>
        /// Called by the client when a new player is connected.
        /// </summary>
        /// <param name="player">The new player which was connected</param>
        public void NotifyNewUserConnected(PokerEngine.Player player)
        {

        }

        /// <summary>
        /// Called by the client when a player performs an action.
        /// </summary>
        /// <param name="player">The player which performed the action</param>
        /// <param name="betAction">The action performed</param>
        /// <param name="callAmount">The call amount if any, can be 0</param>
        /// <param name="raiseAmount">The raise amount if any, can be 0</param>
        public void NotifyPlayerAction(PokerEngine.Player player, PokerEngine.Engine.BetAction betAction, int callAmount, int raiseAmount)
        {

        }

        /// <summary>
        /// Called when a player is thinking of a move
        /// </summary>
        /// <param name="thinkingPlayer">The player which is thinking</param>
        public void NotifyPlayerIsThinking(PokerEngine.Player thinkingPlayer)
        {

        }

        /// <summary>
        /// Called by the clien when a player wins/loses.
        /// </summary>
        /// <param name="player">The player which won/lost</param>
        /// <param name="isWinner">True - the player won. False - The player lost.
        /// </param>
        public void NotifyPlayerStatus(PokerEngine.Player player, bool isWinner)
        {

        }

        /// <summary>
        /// Called by the client after all of the results arrived
        /// </summary>
        public void NotifyResultsIncomingCompleted()
        {

        }

        /// <summary>
        /// Called by the client before the results starts to arrive.
        /// </summary>
        public void NotifyResultsIncomingStarted()
        {

        }

        /// <summary>
        /// Called by the client when a server identifies the running game.
        /// </summary>
        /// <param name="serverGame">The type of game the server is running</param>
        /// <param name="connectedPlayers">The number of players currently connected to the server</param>
        public void NotifyRunningGame(ServerGame serverGame, int connectedPlayers)
        {

        }

        /// <summary>
        /// Called by the client when the current client can't participate in the current round.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifyStartingGame"/> is called prior to the round start.
        /// </remarks>
        public void NotifySittingOut()
        {

        }

        /// <summary>
        /// Called by the client when the current round starts and the client is participating.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifySittingOut"/> is called prior to the round start.
        /// </remarks>
        public void NotifyStartingGame()
        {

        }

        /// <summary>
        /// Called by the client when each round ends. 
        /// </summary>
        /// <param name="gameOver">A flag which indicates the game is over.</param>
        /// <param name="winner">The winning player (if any), may be null</param>
        public void WaitOnRoundEnd(bool gameOver, PokerEngine.Player winner)
        {

        }

        /// <summary>
        /// Called by the client to get the current player betting action. Derived classes must implement it and respond with a 
        /// proper action
        /// </summary>
        /// <param name="player">The player which needs to bet</param>
        /// <param name="action">The action to use to pass the player response</param>
        public void WaitPlayerBettingAction(PokerEngine.Player player, PokerEngine.Engine.PlayerBettingAction action)
        {

        }

        /// <summary>
        /// Called by the client when an update message arrives.
        /// </summary>
        /// <param name="player">The players sorted by their round order</param>
        /// <param name="potInformation">The current state of the pot</param>
        /// <param name="communityCards">The community cards in the game (if any) may be null or in 0 length</param>
        public void WaitSynchronization(IEnumerable<Player> player, PotInformation potInformation, Card[] communityCards)
        {

        }

        #endregion

        #region IFiveCardClientHelper Members

        /// <summary>
        /// Called by the client when a drawing round starts
        /// </summary>
        public void NotifyDrawingRoundStarted()
        {

        }

        /// <summary>
        /// Called by the client when a drawing round completes
        /// </summary>
        public void NotifyDrawingRoundCompleted()
        {

        }

        /// <summary>
        /// Called by the client to notify how many cards were drawn by the player
        /// </summary>
        /// <param name="player">The drawing players</param>
        /// <param name="drawCount">The amount of cards drawn. Can be 0</param>
        public void NotifyPlayerDraws(Player player, int drawCount)
        {

        }

        /// <summary>
        /// Updates the current client with the new cards drawn.
        /// </summary>
        /// <param name="player">
        /// The player with the new updated cards
        /// </param>
        public void NotifyPlayerNewCards(Player player)
        {

        }

        /// <summary>
        /// Called by the client to get the player drawing response.
        /// </summary>
        /// <param name="player">The player which draws the cards</param>
        /// <param name="action">The action which should be modified according to the player actions</param>
        public void WaitPlayerDrawingAction(Player player, PokerEngine.Engine.PlayerDrawingAction action)
        {

        }

        #endregion

        #region IClientHelper Members


        /// <summary>
        /// Called by the client when a player disconnects
        /// </summary>
        /// <param name="player">The player which was disconnected</param>
        public void NotifyUserDisconnected(Player player)
        {

        }

        /// <summary>
        /// Called by the client to indicate that a player talked
        /// </summary>
        /// <param name="user">The talking user</param>
        /// <param name="message">The message which the user spoke</param>
        public void UserTalked(Player user, string message)
        {

        }

        /// <summary>
        /// Called by the client when the connection to the server is closed
        /// </summary>
        public void NotifyConnectionClosed()
        {
        }

        #endregion
    }
}
