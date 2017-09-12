using System;
using System.Collections.Generic;
using System.Text;
using PokerConsole.Engine;
using PokerEngine;
using PokerEngine.Engine;
using System.Windows.Threading;
using System.Threading;
using PokerRules.Deck;
using PokerService;

namespace UltimatePoker.Engine
{
    /// <summary>
    /// A generic wpf helper for GUI based clients.
    /// </summary>
    /// <remarks>
    /// This client helper passes calls and replies from the communication thread back to the GUI thread while blocking the 
    /// communication thread.
    /// </remarks>
    public class GuiHelper : IClientHelper
    {
        // a flag which indicates the client is running. It is volatile since multiple threads may access it
        private volatile bool running = false;
        // A wait handle which will be used to block the communication thread. It will be signaled by the communication thread
        private ManualResetEvent waitHandle = new ManualResetEvent(false);
        // A wait handle which is given if the current Gui helper manages the server wait handle. may be null
        private EventWaitHandle serverWaitHandle;
        // The concrete helper which the calls are passed to.
        private IGuiClientHelper realHelper;
        // The GUI thread dispatcher is used to transfer calls.
        private Dispatcher dispatcher;
        // a flag which indicates if a request to get the connected players was made
        private bool gotPlayers = false;
        // the client which is used to communicate with the server
        private BaseWcfClient wcfClient;

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="GuiHelper"/> class.</para>
        /// </summary>
        /// <param name="realHelper">The concrete helper which is used to pass concrete calls
        /// </param>
        /// <exception cref="InvalidOperationException">If the calling thread does not have a dispatcher attached.
        /// </exception>
        public GuiHelper(IGuiClientHelper realHelper)
        {
            this.realHelper = realHelper;
            // get the dispatcher of the current thread. Don't want to create a new one.
            dispatcher = Dispatcher.FromThread(System.Threading.Thread.CurrentThread);
            if (dispatcher == null)
                throw new InvalidOperationException("The gui helper can only be used with in the gui thread");
            // register to the helper events to signal the wait handle
            realHelper.GuiClientResponded += new EventHandler(realHelper_GuiClientResponded);

        }

        /// <summary>
        /// Initializes the helper with the client used to communicate with the server
        /// </summary>
        /// <param name="client">The client which connects to the server</param>
        /// <param name="canChat">A flag indicating if the client allows the user to talk</param>
        /// <param name="serverWaitHandle">A wait handle which is given if the current Gui helper manages the server wait handle. may be null</param>
        public void Initialize(BaseWcfClient client, bool canChat, EventWaitHandle serverWaitHandle)
        {
            this.wcfClient = client;
            this.serverWaitHandle = serverWaitHandle;
            // register to game cancelation
            realHelper.GameAborted += new DataEventHandler<bool>(realHelper_GameAborted);
            if (canChat) // when the chat is enabled, register to the helper chat event
            {
                // this method is usually called on a different thread
                dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                delegate
                {
                    realHelper.UserChats += new DataEventHandler<string>(realHelper_UserChats);
                }));
            }
        }

        /// <summary>
        /// A callback for the <see cref="IGuiClientHelper.UserChats"/> event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void realHelper_UserChats(object sender, DataEventArgs<string> e)
        {
            // start a post back to the server on the thread pool
            ThreadPool.QueueUserWorkItem(postChat, e.Data);
        }

        /// <summary>
        /// A callback for chat message post back
        /// </summary>
        /// <param name="state">The message passed by the client</param>
        private void postChat(object state)
        {
            try
            {
                string message = (string)state;
                wcfClient.PokerRoomChat.Speak(message);
            }
            catch
            {
                // swallow the exception since it will be caught in the right place
            }
        }

        /// <summary>
        /// A callback for the <see cref="IGuiClientHelper.GameAborted"/> event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void realHelper_GameAborted(object sender, DataEventArgs<bool> e)
        {
            // mark the game as not running
            running = false;
            // release any blocking call of the communication thread
            waitHandle.Set();
            if (serverWaitHandle != null)
                serverWaitHandle.Set();
            
            if (e.Data)
                wcfClient.Disconnect();
        }

        /// <summary>
        /// A callback for the <see cref="IGuiClientHelper.GuiClientResponded"/> event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void realHelper_GuiClientResponded(object sender, EventArgs e)
        {
            // release any blocking call of the communication thread
            waitHandle.Set();
        }

        /// <summary>
        /// Called manually by helped classes to signal the communication to continue and receive new messages
        /// </summary>
        internal void Continue()
        {
            waitHandle.Set();
        }
        #region IClientHelper Members

        /// <summary>
        /// Called when the betting round finalizes in any case the round is over.
        /// </summary>
        public void FinalizeBettingRound()
        {
            Invoke(realHelper.FinalizeBettingRound);
        }

        /// <summary>
        /// Gets the login name of the client. This method may be called again after a call to <see cref="NotifyNameExists"/></summary>
        /// <returns>A valid user name to login the client</returns>
        /// <remarks>
        /// This method must aquire the user login name. A new value must be returned after a call to <see cref="NotifyNameExists"/></remarks>
        public string GetName()
        {
            // this method is non blocking
            return (string)dispatcher.Invoke(DispatcherPriority.Normal, new Func<string>(realHelper.GetName));
        }

        /// <summary>
        /// Called when the betting round is completed and there is a single winner.
        /// </summary>
        public void NotifyAllFolded()
        {
            Invoke(realHelper.NotifyAllFolded);
        }

        /// <summary>
        /// Called when a betting round is completed and all players have checked.
        /// </summary>
        public void NotifyBetRoundComplete()
        {
            Invoke(realHelper.NotifyBetRoundComplete);
        }

        /// <summary>
        /// Called before a betting round starts.
        /// </summary>
        public void NotifyBetRoundStarted()
        {
            Invoke(realHelper.NotifyBetRoundStarted);
        }

        /// <summary>
        /// Called by the client when the blind open is made.
        /// </summary>
        /// <param name="opened">The player which blind opens</param>
        /// <param name="openAmount">The player open amount, may be 0</param>
        public void NotifyBlindOpen(Player opened, int openAmount)
        {
            Invoke<Player, int>(realHelper.NotifyBlindOpen, opened, openAmount);
        }

        /// <summary>
        /// Called by the client when the blind raise is made.
        /// </summary>
        /// <param name="raiser">The player which blind raises</param>
        /// <param name="raiseAmount">The raise amound, may be 0</param>
        public void NotifyBlindRaise(Player raiser, int raiseAmount)
        {
            Invoke<Player, int>(realHelper.NotifyBlindRaise, raiser, raiseAmount);
        }

        /// <summary>
        /// Called by the client when a connection is made successfuly to the server
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint which was opened locally.
        /// </param>
        public void NotifyConnectedToServer(System.Net.EndPoint endPoint)
        {
            running = true;
            Invoke<System.Net.EndPoint>(realHelper.NotifyConnectedToServer, endPoint);
        }

        /// <summary>
        /// Called by the client when the connection to the server is closed
        /// </summary>
        public void NotifyConnectionClosed()
        {
            Invoke(realHelper.NotifyConnectionClosed);
        }

        /// <summary>
        /// Called by the client when a round starts.
        /// </summary>
        /// <param name="dealer">The current round dealer</param>
        /// <param name="potAmount">The starting amount of money in the pot</param>
        public void NotifyDealerAndPotAmount(Player dealer, int potAmount)
        {
            Invoke<Player, int>(realHelper.NotifyDealerAndPotAmount, dealer, potAmount);
        }

        /// <summary>
        /// Called by the client when a game is canceled by the server
        /// </summary>
        public void NotifyGameCanceled()
        {
            if (serverWaitHandle != null)
                serverWaitHandle.Set();
            Invoke(realHelper.NotifyGameCanceled);
        }

        /// <summary>
        /// Called by the client when a game is already in progress. The server can't be connected
        /// </summary>
        public void NotifyGameInProgress()
        {
            Invoke(realHelper.NotifyGameInProgress);
        }

        /// <summary>
        /// Called by the client when the name returned by <see cref="GetName"/> already exists on the server.
        /// </summary>
        /// <remarks>
        /// Derived classes may use this method to create a new login name.
        /// </remarks>
        public void NotifyNameExists()
        {
            Invoke(realHelper.NotifyNameExists);
        }

        /// <summary>
        /// Called by the client when a new player is connected.
        /// </summary>
        /// <param name="player">The new player which was connected</param>
        public void NotifyNewUserConnected(Player player)
        {
            Invoke<Player>(realHelper.NotifyNewUserConnected, player);
            if (!gotPlayers)
            {
                gotPlayers = true;
                IEnumerable<Player> loggedInPlayers = wcfClient.PokerHost.GetLoggedinPlayers();
                Invoke<IEnumerable<Player>>(realHelper.NotifyLoggedinPlayers, loggedInPlayers);
            }

        }

        /// <summary>
        /// Called by the client when a player performs an action.
        /// </summary>
        /// <param name="player">The player which performed the action</param>
        /// <param name="betAction">The action performed</param>
        /// <param name="callAmount">The call amount if any, can be 0</param>
        /// <param name="raiseAmount">The raise amount if any, can be 0</param>
        public void NotifyPlayerAction(Player player, BetAction betAction, int callAmount, int raiseAmount)
        {
            Invoke<Player, BetAction, int, int>(realHelper.NotifyPlayerAction, player, betAction, callAmount, raiseAmount);
        }

        /// <summary>
        /// Called when a player is thinking of a move
        /// </summary>
        /// <param name="thinkingPlayer">The player which is thinking</param>
        public void NotifyPlayerIsThinking(Player thinkingPlayer)
        {
            Invoke<Player>(realHelper.NotifyPlayerIsThinking, thinkingPlayer);
        }

        /// <summary>
        /// Called by the clien when a player wins/loses.
        /// </summary>
        /// <param name="player">The player which won/lost</param>
        /// <param name="isWinner">True - the player won. False - The player lost.
        /// </param>
        public void NotifyPlayerStatus(Player player, bool isWinner)
        {
            Invoke<Player, bool>(realHelper.NotifyPlayerStatus, player, isWinner);
        }

        /// <summary>
        /// Called by the client after all of the results arrived
        /// </summary>
        public void NotifyResultsIncomingCompleted()
        {
            Invoke(realHelper.NotifyResultsIncomingCompleted);
        }

        /// <summary>
        /// Called by the client before the results starts to arrive.
        /// </summary>
        public void NotifyResultsIncomingStarted()
        {
            Invoke(realHelper.NotifyResultsIncomingStarted);
        }

        /// <summary>
        /// Called by the client when a server identifies the running game.
        /// </summary>
        /// <param name="serverGame">The type of game the server is running</param>
        /// <param name="connectedPlayers">The number of players currently connected to the server</param>
        public void NotifyRunningGame(ServerGame serverGame, int connectedPlayers)
        {
            Invoke<ServerGame, int>(realHelper.NotifyRunningGame, serverGame, connectedPlayers);

        }

        /// <summary>
        /// Called by the client when the current client can't participate in the current round.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifyStartingGame"/> is called prior to the round start.
        /// </remarks>
        public void NotifySittingOut()
        {
            Invoke(realHelper.NotifySittingOut);
        }

        /// <summary>
        /// Called by the client when the current round starts and the client is participating.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifySittingOut"/> is called prior to the round start.
        /// </remarks>
        public void NotifyStartingGame()
        {
            Invoke(realHelper.NotifyStartingGame);
        }

        /// <summary>
        /// Called by the client to notify the current hand which is played
        /// </summary>
        /// <param name="currentHand">The current hand</param>
        public void NotifyCurrentHand(int currentHand)
        {
            Invoke<int>(realHelper.NotifyCurrentHand, currentHand);
        }


        /// <summary>
        /// Called by the client when each round ends. 
        /// </summary>
        /// <param name="gameOver">A flag which indicates the game is over.</param>
        /// <param name="winner">The winning player (if any), may be null</param>
        public void WaitOnRoundEnd(bool gameOver, Player winner)
        {
            Invoke<bool, Player>(realHelper.WaitOnRoundEnd, gameOver, winner);
            if (gameOver && serverWaitHandle != null)
            {
                if (winner == null)
                    serverWaitHandle.Set();
                else
                    Invoke(realHelper.NotifyServerWaitsToClose);
            }


        }

        /// <summary>
        /// Called by the client to get the current player betting action. Derived classes must implement it and respond with a 
        /// proper action
        /// </summary>
        /// <param name="player">The player which needs to bet</param>
        /// <param name="action">The action to use to pass the player response</param>
        public void WaitPlayerBettingAction(Player player, PlayerBettingAction action)
        {
            CheckWaitLimit();
            Invoke<Player, PlayerBettingAction>(realHelper.WaitPlayerBettingAction, player, action);
        }


        /// <summary>
        /// Called by the client when an update message arrives.
        /// </summary>
        /// <param name="player">The players sorted by their round order</param>
        /// <param name="potInformation">The current state of the pot</param>
        /// <param name="communityCards">The community cards in the game (if any) may be null or in 0 length</param>
        public void WaitSynchronization(IEnumerable<Player> player, PokerService.PotInformation potInformation, Card[] communityCards)
        {
            Invoke<IEnumerable<Player>, PokerService.PotInformation, Card[]>(realHelper.WaitSynchronization, player, potInformation, communityCards);
        }

        /// <summary>
        /// Called by the client when a player disconnects
        /// </summary>
        /// <param name="player">The player which was disconnected</param>
        public void NotifyUserDisconnected(Player player)
        {
            Invoke<Player>(realHelper.NotifyUserDisconnected, player);
        }

        /// <summary>
        /// Called by the client to indicate that a player talked
        /// </summary>
        /// <param name="user">The talking user</param>
        /// <param name="message">The message which the user spoke</param>
        public void UserTalked(Player user, string message)
        {
            dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<Player, string>(realHelper.UserTalked), user, message);
            //Invoke<Player, string>(realHelper.UserTalked, user, message);
        }

        #endregion

        /// <summary>
        /// A method which is used to check for a time limit and call the <see cref="IGuiClientHelper.StartActionTimer"/>
        /// when a limit exists
        /// </summary>
        /// <remarks>
        /// Dervied classes should call this method prior to any wait operations passed to the <see cref="IGuiClientHelper"/>
        /// </remarks>
        protected void CheckWaitLimit()
        {
            TimeSpan currentLimit = wcfClient.PokerService.GetCurrentActionDeadLine();
            if (currentLimit > TimeSpan.Zero)
            {
                dispatcher.BeginInvoke(DispatcherPriority.Send, new Action<TimeSpan>(realHelper.StartActionTimer), currentLimit);
            }
        }


        /// <summary>
        /// Checks the game is running and the user didn't abort the game.
        /// </summary>
        private void checkRunning()
        {
            // notify the communiication client that the game was aborted
            if (!running)
            {
                wcfClient.Disconnect();
            }
            else
            {
                // if the game is running, can reset the handle for the GUI response
                waitHandle.Reset();
            }
        }

        /// <summary>
        /// Waits for the concrete GUI helper to respond. This should be called by the communication thread to wait for the user 
        /// actions
        /// </summary>
        private void waitForOpeartion()
        {
            //operation.Wait();

            waitHandle.WaitOne();

            //operation = null;
        }

        /// <summary>
        /// Used to invoke a general <see cref="Action"/> on the gui thread. This will block until the client will respond.
        /// </summary>
        /// <param name="action">The action to execute on the GUI thread</param>
        protected void Invoke(Action action)
        {
            checkRunning();

            dispatcher.Invoke(DispatcherPriority.Normal, action);

            waitForOpeartion();
        }


        /// <summary>
        /// Used to invoke a general <see cref="Action{T}"/> on the gui thread. This will block until the client will respond.
        /// </summary>
        /// <typeparam name="T">The type of the arugment which is passed to the action</typeparam>
        /// <param name="action">The action to execute on the GUI thread</param>
        /// <param name="t">The argument to pass to the action</param>
        protected void Invoke<T>(Action<T> action, T t)
        {
            checkRunning();

            dispatcher.Invoke(DispatcherPriority.Normal, action, t);

            waitForOpeartion();
        }

        /// <summary>
        /// Used to invoke a general <see cref="Action{T,U}"/> on the gui thread. This will block until the client will respond.
        /// </summary>
        /// <typeparam name="T">The type of the first arugment which is passed to the action</typeparam>
        /// <typeparam name="U">The type of the second arugment which is passed to the action</typeparam>
        /// <param name="action">The action to execute on the GUI thread</param>
        /// <param name="t">The first argument to pass to the action</param>
        /// <param name="u">The second argument to pass to the action </param>
        protected void Invoke<T, U>(Action<T, U> action, T t, U u)
        {
            checkRunning();

            dispatcher.Invoke(DispatcherPriority.Normal, action, t, u);

            waitForOpeartion();
        }

        /// <summary>
        /// Used to invoke a general <see cref="Action{T,U}"/> on the gui thread. This will block until the client will respond.
        /// </summary>
        /// <typeparam name="T">The type of the first arugment which is passed to the action</typeparam>
        /// <typeparam name="U">The type of the second arugment which is passed to the action</typeparam>
        /// <typeparam name="V">The type of the third arugment which is passed to the action</typeparam>
        /// <param name="action">The action to execute on the GUI thread</param>
        /// <param name="t">The first argument to pass to the action</param>
        /// <param name="u">The second argument to pass to the action </param>
        /// <param name="v">The third argument to pass to the action </param>
        protected void Invoke<T, U, V>(Action<T, U, V> action, T t, U u, V v)
        {
            checkRunning();

            dispatcher.Invoke(DispatcherPriority.Normal, action, t, u, v);

            waitForOpeartion();
        }

        /// <summary>
        /// Used to invoke a general <see cref="Action{T,U}"/> on the gui thread. This will block until the client will respond.
        /// </summary>
        /// <typeparam name="T">The type of the first arugment which is passed to the action</typeparam>
        /// <typeparam name="U">The type of the second arugment which is passed to the action</typeparam>
        /// <typeparam name="V">The type of the third arugment which is passed to the action</typeparam>
        /// <typeparam name="W">The type of the fourth arugment which is passed to the action</typeparam>
        /// <param name="action">The action to execute on the GUI thread</param>
        /// <param name="t">The first argument to pass to the action</param>
        /// <param name="u">The second argument to pass to the action </param>
        /// <param name="v">The third argument to pass to the action </param>
        /// <param name="w">The fourth argument to pass to the action </param>
        protected void Invoke<T, U, V, W>(Action<T, U, V, W> action, T t, U u, V v, W w)
        {
            checkRunning();

            dispatcher.Invoke(DispatcherPriority.Normal, action, t, u, v, w);

            waitForOpeartion();
        }



    }
}
