using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerEngine;
using PokerEngine.Engine;
using PokerRules.Deck;
using PokerService;

namespace PokerConsole.Engine
{
    /// <summary>
    /// An abstract base class for <see cref="IClientHelper"/> based binary clients.
    /// </summary>
    /// <remarks>
    /// This class provides decorative support for clients which uses a <see cref="IClientHelper"/>
    /// </remarks>
    public abstract class ClientHelperDecorator : IClientHelper
    {
        // the helper used to implement the client interaction
        private IClientHelper helper;

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="ClientHelperDecorator"/> class.</para>
        /// </summary>
        /// <param name="helper">The helper to use. May be null but must manually set the <see cref="Helper"/> property
        /// </param>
        protected ClientHelperDecorator(IClientHelper helper)
        {
            this.helper = helper;
        }
        /// <summary>
        /// Gets or sets the used helper. May not be null
        /// </summary>
        protected IClientHelper Helper
        {
            get { return helper; }
            set { helper = value; }
        }

        /// <summary>
        /// Finds a wrapped helper down the chain of helpers or null if none exists
        /// </summary>
        /// <typeparam name="T">The type of the helper to find</typeparam>
        /// <returns>The first wrapped helper of type <typeparamref name="T"/> which is found, or default of <typeparamref name="T"/>
        /// when none found.
        /// </returns>
        public T FindWrappedHelper<T>() where T : IClientHelper
        {
            // check if current helper is of the correct type
            if (helper is T)
                return (T)helper;
            // search downward
            ClientHelperDecorator decorator = helper as ClientHelperDecorator;
            if (decorator == null)
                return default(T); // none found
            return decorator.FindWrappedHelper<T>();
        }

        #region IClientHelper Members

        /// <summary>
        /// Called by the client to notify the current hand which is played
        /// </summary>
        /// <param name="currentHand">The current hand</param>
        public virtual void NotifyCurrentHand(int currentHand)
        {
            helper.NotifyCurrentHand(currentHand);
        }

        /// <summary>
        /// Called when the betting round finalizes in any case the round is over.
        /// </summary>
        public virtual void FinalizeBettingRound()
        {
            helper.FinalizeBettingRound();
        }

        /// <summary>
        /// Gets the login name of the client. This method may be called again after a call to <see cref="NotifyNameExists"/></summary>
        /// <returns>A valid user name to login the client</returns>
        /// <remarks>
        /// This method must aquire the user login name. A new value must be returned after a call to <see cref="NotifyNameExists"/></remarks>
        public virtual string GetName()
        {
            return helper.GetName();
        }

        /// <summary>
        /// Called when the betting round is completed and there is a single winner.
        /// </summary>
        public virtual void NotifyAllFolded()
        {
            helper.NotifyAllFolded();
        }

        /// <summary>
        /// Called when a betting round is completed and all players have checked.
        /// </summary>
        public virtual void NotifyBetRoundComplete()
        {
            helper.NotifyBetRoundComplete();
        }

        /// <summary>
        /// Called before a betting round starts.
        /// </summary>
        public virtual void NotifyBetRoundStarted()
        {
            helper.NotifyBetRoundStarted();
        }

        /// <summary>
        /// Called by the client when the blind open is made.
        /// </summary>
        /// <param name="opened">The player which blind opens</param>
        /// <param name="openAmount">The player open amount, may be 0</param>
        public virtual void NotifyBlindOpen(Player opened, int openAmount)
        {
            helper.NotifyBlindOpen(opened, openAmount);
        }

        /// <summary>
        /// Called by the client when the blind raise is made.
        /// </summary>
        /// <param name="raiser">The player which blind raises</param>
        /// <param name="raiseAmount">The raise amound, may be 0</param>
        public virtual void NotifyBlindRaise(Player raiser, int raiseAmount)
        {
            helper.NotifyBlindRaise(raiser, raiseAmount);
        }

        /// <summary>
        /// Called by the client when a connection is made successfuly to the server
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint which was opened locally.
        /// </param>
        public virtual void NotifyConnectedToServer(System.Net.EndPoint endPoint)
        {
            helper.NotifyConnectedToServer(endPoint);
        }

        /// <summary>
        /// Called by the client when a round starts.
        /// </summary>
        /// <param name="dealer">The current round dealer</param>
        /// <param name="potAmount">The starting amount of money in the pot</param>
        public virtual void NotifyDealerAndPotAmount(Player dealer, int potAmount)
        {
            helper.NotifyDealerAndPotAmount(dealer, potAmount);
        }

        /// <summary>
        /// Called by the client when a game is canceled by the server
        /// </summary>
        public virtual void NotifyGameCanceled()
        {
            helper.NotifyGameCanceled();
        }

        /// <summary>
        /// Called by the client when a game is already in progress. The server can't be connected
        /// </summary>
        public virtual void NotifyGameInProgress()
        {
            helper.NotifyGameInProgress();
        }

        /// <summary>
        /// Called by the client when the name returned by <see cref="GetName"/> already exists on the server.
        /// </summary>
        /// <remarks>
        /// Derived classes may use this method to create a new login name.
        /// </remarks>
        public virtual void NotifyNameExists()
        {
            helper.NotifyNameExists();
        }

        /// <summary>
        /// Called by the client when a new player is connected.
        /// </summary>
        /// <param name="player">The new player which was connected</param>
        public virtual void NotifyNewUserConnected(Player player)
        {
            helper.NotifyNewUserConnected(player);
        }

        /// <summary>
        /// Called by the client when a player performs an action.
        /// </summary>
        /// <param name="player">The player which performed the action</param>
        /// <param name="betAction">The action performed</param>
        /// <param name="callAmount">The call amount if any, can be 0</param>
        /// <param name="raiseAmount">The raise amount if any, can be 0</param>
        public virtual void NotifyPlayerAction(Player player, BetAction betAction, int callAmount, int raiseAmount)
        {
            helper.NotifyPlayerAction(player, betAction, callAmount, raiseAmount);
        }

        /// <summary>
        /// Called when a player is thinking of a move
        /// </summary>
        /// <param name="thinkingPlayer">The player which is thinking</param>
        public virtual void NotifyPlayerIsThinking(Player thinkingPlayer)
        {
            helper.NotifyPlayerIsThinking(thinkingPlayer);
        }

        /// <summary>
        /// Called by the clien when a player wins/loses.
        /// </summary>
        /// <param name="player">The player which won/lost</param>
        /// <param name="isWinner">True - the player won. False - The player lost.
        /// </param>
        public virtual void NotifyPlayerStatus(Player player, bool isWinner)
        {
            helper.NotifyPlayerStatus(player, isWinner);
        }

        /// <summary>
        /// Called by the client after all of the results arrived
        /// </summary>
        public virtual void NotifyResultsIncomingCompleted()
        {
            helper.NotifyResultsIncomingCompleted();
        }

        /// <summary>
        /// Called by the client before the results starts to arrive.
        /// </summary>
        public virtual void NotifyResultsIncomingStarted()
        {
            helper.NotifyResultsIncomingStarted();
        }

        /// <summary>
        /// Called by the client when a server identifies the running game.
        /// </summary>
        /// <param name="serverGame">The type of game the server is running</param>
        /// <param name="connectedPlayers">The number of players currently connected to the server</param>
        public virtual void NotifyRunningGame(ServerGame serverGame, int connectedPlayers)
        {
            helper.NotifyRunningGame(serverGame, connectedPlayers);
        }

        /// <summary>
        /// Called by the client when the current client can't participate in the current round.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifyStartingGame"/> is called prior to the round start.
        /// </remarks>
        public virtual void NotifySittingOut()
        {
            helper.NotifySittingOut();
        }

        /// <summary>
        /// Called by the client when the current round starts and the client is participating.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifySittingOut"/> is called prior to the round start.
        /// </remarks>
        public virtual void NotifyStartingGame()
        {
            helper.NotifyStartingGame();
        }

        /// <summary>
        /// Called by the client when each round ends. 
        /// </summary>
        /// <param name="gameOver">A flag which indicates the game is over.</param>
        /// <param name="winner">The winning player (if any), may be null</param>
        public virtual void WaitOnRoundEnd(bool gameOver, Player winner)
        {
            helper.WaitOnRoundEnd(gameOver, winner);
        }

        /// <summary>
        /// Called by the client to get the current player betting action. Derived classes must implement it and respond with a 
        /// proper action
        /// </summary>
        /// <param name="player">The player which needs to bet</param>
        /// <param name="action">The action to use to pass the player response</param>
        public virtual void WaitPlayerBettingAction(Player player, PlayerBettingAction action)
        {
            helper.WaitPlayerBettingAction(player, action);
        }

        /// <summary>
        /// Called by the client when an update message arrives.
        /// </summary>
        /// <param name="player">The players sorted by their round order</param>
        /// <param name="potInformation">The current state of the pot</param>
        /// <param name="communityCards">The community cards in the game (if any) may be null or in 0 length</param>
        public virtual void WaitSynchronization(IEnumerable<Player> player, PotInformation potInformation, Card[] communityCards)
        {
            helper.WaitSynchronization(player, potInformation, communityCards);
        }




        /// <summary>
        /// Called by the client when the connection to the server is closed
        /// </summary>
        public virtual void NotifyConnectionClosed()
        {
            helper.NotifyConnectionClosed();
        }

        /// <summary>
        /// Called by the client when a player disconnects
        /// </summary>
        /// <param name="player">The player which was disconnected</param>
        public virtual void NotifyUserDisconnected(Player player)
        {
            helper.NotifyUserDisconnected(player);
        }

        /// <summary>
        /// Called by the client to indicate that a player talked
        /// </summary>
        /// <param name="user">The talking user</param>
        /// <param name="message">The message which the user spoke</param>
        public virtual void UserTalked(Player user, string message)
        {
            helper.UserTalked(user, message);
        }

        #endregion
    }
}
