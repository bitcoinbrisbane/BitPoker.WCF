using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerRules.Games;
using PokerConsole.Engine;
using PokerService;
using System.ServiceModel;
using System.ServiceModel.Channels;
using PokerEngine;
using PokerEngine.Engine;
using BitPoker.Models.Deck;
using System.Threading;

namespace PokerConsole.Engine
{
    /// <summary>
    /// The basic implementation of the Wcf Poker client
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class BaseWcfClient : IPokerClient, IDisposable
    {

        // The wait handle is used for blocking scenarios which uses the Run() method
        private ManualResetEvent waitHandle = null;
        // a flag which indicates the client can connect to the server
        private bool canConnect = true;
        // The client which is used to provide user input/output
        private IClientHelper concreteClient;
        // The client (with additional five cards methods) which is used to provide user input/output
        private IFiveCardClientHelper fiveCardConcreteClient;
        // The proxy to the poker service
        private PokerServiceProxy proxyService;
        // The proxy to poker host
        private PokerHostProxy pokerHost;
        // the proxy to the poker broadcast
        private PokerBroadcastProxy broadcastProxy;
        // the proxy to the chat service
        private PokerRoomChatProxy chatProxy;
        // the chat client callback
        private PokerChatClient chatClient;
        // the list of all opened proxies
        private List<ICommunicationObject> proxies = new List<ICommunicationObject>(4);
        // a flag which indicates the game is running
        private bool running = false;
        // a flag which indicates if needs to call ClientHelper.ResultsIncomingStarted
        private bool notifyResultIncomingStarted = true;
        // a flag which indicates if needs to call ClientHelper.BetRoundStarted
        private bool betRoundStarting = true;
        // a flag which indicates if needs to call ClientHelper.DrawingRoundStarted
        private bool drawingRoundStarted = true;
        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="BaseWcfClient"/> class.</para>
        /// </summary>
        /// <param name="concreteClient">The client which is used to provide user input/output
        /// </param>
        public BaseWcfClient(IClientHelper concreteClient)
        {
            this.concreteClient = concreteClient;
        }

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="BaseWcfClient"/> class.</para>
        /// </summary>
        /// <param name="fiveCardConcreteClient">The client which is used to provide user input/output
        /// </param>
        public BaseWcfClient(IFiveCardClientHelper fiveCardConcreteClient)
        {
            this.concreteClient = fiveCardConcreteClient;
            this.fiveCardConcreteClient = fiveCardConcreteClient;
        }

        /// <summary>
        /// Gets the concrete client which is used to provide user input/output
        /// </summary>
        public IClientHelper ConcreteClient { get { return concreteClient; } }

        /// <summary>
        /// Gets the five card client which is used to provide user input/output (may be null)
        /// </summary>
        /// <remarks>
        /// This property has a value when the <see cref="BaseWcfClient(IFiveCardClientHelper)"/>
        /// </remarks>
        public IFiveCardClientHelper FiveCardConcreteClient { get { return fiveCardConcreteClient; } }

        /// <summary>
        /// Initializes the client. Opens connection to the specified server
        /// </summary>
        /// <param name="ipOrDns">The server IP or DNS address</param>
        /// <param name="port">The server port</param>
        /// <returns>
        /// The connected server details
        /// </returns>
        public ServerDetails Initialize(string ipOrDns, int port)
        {
            // the context with the current client
            InstanceContext context = new InstanceContext(this);
            // setup service channel
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None, true);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            binding.SendTimeout = TimeSpan.FromMinutes(5);
            binding.ReliableSession.Ordered = true;
            EndpointAddress address = new EndpointAddress(string.Format("net.tcp://{0}:{1}/PokerService", ipOrDns, port));
            proxyService = new PokerServiceProxy(context, binding, address);


            // setup host channel
            binding = new NetTcpBinding(SecurityMode.None);
            address = new EndpointAddress(string.Format("net.tcp://{0}:{1}/PokerHost", ipOrDns, port));
            pokerHost = new PokerHostProxy(binding, address);
            pokerHost.Open();
            proxies.Add(pokerHost);

            // setup the broadcast channel
            binding = new NetTcpBinding(SecurityMode.None, true);
            binding.ReliableSession.Ordered = true;
            address = new EndpointAddress(string.Format("net.tcp://{0}:{1}/PokerServiceBroadcast", ipOrDns, port));
            broadcastProxy = new PokerBroadcastProxy(context, binding, address);


            // setup the chat channel
            chatClient = new PokerChatClient(concreteClient);
            InstanceContext chatContext = new InstanceContext(chatClient);
            binding = new NetTcpBinding(SecurityMode.None);
            address = new EndpointAddress(string.Format("net.tcp://{0}:{1}/PokerRoomChat", ipOrDns, port));
            chatProxy = new PokerRoomChatProxy(chatContext, binding, address);


            // notify client of the connection
            concreteClient.NotifyConnectedToServer(new System.Net.IPEndPoint(System.Net.Dns.GetHostAddresses(ipOrDns)[0], port));
            ServerDetails result = pokerHost.GetServerDetails();
            concreteClient.NotifyRunningGame(result.Game, result.ConnectedPlayers);
            if (result.Game == ServerGame.FiveCardDraw && fiveCardConcreteClient == null)
            {
                result.CanConnect = canConnect = false;
            }
            return result;
        }

        /// <summary>
        /// A callback which is called when the service channel closes/faults. It marks the client as not connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InnerChannel_Faulted(object sender, EventArgs e)
        {
            IClientChannel channel = (IClientChannel)sender;
            // if the channel faults, notfiy cancealation
            if (channel.State == CommunicationState.Faulted)
                concreteClient.NotifyGameCanceled();
            else
                concreteClient.NotifyConnectionClosed();
            // in any case, mark as not running
            canConnect = false;
            Running = false;

        }

        /// <summary>
        /// Logins to the poker service.
        /// </summary>
        /// <remarks>
        /// This method must be called after a call to <see cref="Initialize"/>
        /// </remarks>
        public void Connect()
        {
            // assure initialized
            CheckInitialized(proxyService);
            CheckInitialized(chatProxy);

            // check that the server didn't refuse the connection already
            if (canConnect)
            {
                // get the user name
                string userName = concreteClient.GetName();
                // try to login (the server might call NotifyNameExists or NotifyGameInProgress)
                Player loggedIn = proxyService.Login(userName);
                // when the server replies with a null result, try a new login name...
                if (loggedIn == null)
                {
                    Connect();
                }
                else // otherwise, the player logged in, try to sign in to the chat session
                {
                    chatProxy.Login(proxyService.InnerChannel.SessionId);
                    Running = true;
                }
            }
        }


        /// <summary>
        /// Disconnects the client from the server
        /// </summary>
        public void Disconnect()
        {
            // try to disconnect each channel
            foreach (ICommunicationObject cur in proxies)
            {
                try
                {
                    if (cur.State == CommunicationState.Opened)
                    {
                        cur.Close();
                    }
                }
                catch { }
            }
            Running = false;

        }

        /// <summary>
        /// This method provides clients with a blocking method which blocks until the game is over (one way or another)
        /// </summary>
        /// <remarks>
        /// This method should be called by threads who needs to block until the game ends.
        /// </remarks>
        public void Run()
        {
            CheckInitialized(proxyService);
            if (proxyService.State == CommunicationState.Opened)
            {
                if (waitHandle == null)
                {
                    waitHandle = new ManualResetEvent(!canConnect);
                }
                waitHandle.WaitOne();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the the client is running
        /// </summary>
        private bool Running
        {
            get { return running; }
            set
            {
                if (running != value)
                {
                    this.running = value;
                    // check if needs to signal a waiting handle
                    if (!running && waitHandle != null)
                    {
                        waitHandle.Set();
                    }
                }
            }
        }

        /// <summary>
        /// Requests permissions to spectate the running game
        /// </summary>
        /// <returns>
        /// True if the client can watch, false otherwise.
        /// </returns>
        public bool RequestSpectation()
        {
            CheckInitialized(broadcastProxy);
            return broadcastProxy.RequestSpectation();
        }

        /// <summary>
        /// Gets the connected poker host
        /// </summary>
        public IPokerHost PokerHost { get { return pokerHost; } }

        /// <summary>
        /// Gets the connected poker service
        /// </summary>
        public IPokerService PokerService { get { return proxyService; } }

        /// <summary>
        /// Gets the connected poker broadcast service
        /// </summary>
        public IPokerServiceBroadcast PokerServiceBroadcast { get { return broadcastProxy; } }

        /// <summary>
        /// Gets the connected poker chat service
        /// </summary>
        public IPokerRoomChat PokerRoomChat { get { return chatProxy; } }

        /// <summary>
        /// Checks that the service is initialized and raises an exception when it isn't
        /// </summary>
        /// <param name="comObject">The communication object to verify initialization</param>
        /// <returns>True if the object is initialized, false if the object was opened</returns>
        /// <exception cref="InvalidOperationException">When the client didn't initialize
        /// </exception>
        private bool CheckInitialized(ICommunicationObject comObject)
        {
            if (comObject == null)
                throw new InvalidOperationException("Must call Initialize first");
            else if (comObject.State == CommunicationState.Created)
            {
                comObject.Open();
                proxies.Add(comObject);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks that the service is initialized.
        /// </summary>
        /// <param name="pokerService">The service to verify intitialization</param>
        private void CheckInitialized(PokerServiceProxy pokerService)
        {
            if (!CheckInitialized((ICommunicationObject)pokerService))
            {
                pokerService.InnerChannel.Faulted += new EventHandler(InnerChannel_Faulted);
                pokerService.InnerChannel.Closed += new EventHandler(InnerChannel_Faulted);
                
            }
        }

        /// <summary>
        /// Checks that the service is initialized.
        /// </summary>
        /// <param name="broadcastProxy">The service to verify intitialization</param>
        private void CheckInitialized(PokerBroadcastProxy broadcastProxy)
        {
            if (!CheckInitialized((ICommunicationObject)broadcastProxy))
            {
                broadcastProxy.InnerChannel.Faulted += new EventHandler(InnerChannel_Faulted);
                broadcastProxy.InnerChannel.Closed += new EventHandler(InnerChannel_Faulted);
            }
        }

        #region IPokerClient Members

        /// <summary>
        /// Called when the betting round finalizes in any case the round is over.
        /// </summary>
        public void FinalizeBettingRound()
        {
            betRoundStarting = true;
            concreteClient.FinalizeBettingRound();
        }

        /// <summary>
        /// Called when the betting round is completed and there is a single winner.
        /// </summary>
        public void NotifyAllFolded()
        {
            concreteClient.NotifyAllFolded();
        }
        /// <summary>
        /// Called when a betting round is completed and all players have checked.
        /// </summary>
        public void NotifyBetRoundComplete()
        {
            concreteClient.NotifyBetRoundComplete();
        }

        /// <summary>
        /// Called by the client when the blind open is made.
        /// </summary>
        /// <param name="opened">The player which blind opens</param>
        /// <param name="openAmount">The player open amount, may be 0</param>
        public void NotifyBlindOpen(Player opened, int openAmount)
        {
            concreteClient.NotifyBlindOpen(opened, openAmount);
        }

        /// <summary>
        /// Called by the client when the blind raise is made.
        /// </summary>
        /// <param name="raiser">The player which blind raises</param>
        /// <param name="raiseAmount">The raise amound, may be 0</param>
        public void NotifyBlindRaise(Player raiser, int raiseAmount)
        {
            concreteClient.NotifyBlindRaise(raiser, raiseAmount);
        }

        /// <summary>
        /// Called by the client to notify the current hand which is played
        /// </summary>
        /// <param name="currentHand">The current hand</param>
        public void NotifyCurrentHand(int currentHand)
        {
            concreteClient.NotifyCurrentHand(currentHand);
        }

        /// <summary>
        /// Called by the client when a round starts.
        /// </summary>
        /// <param name="dealer">The current round dealer</param>
        /// <param name="potAmount">The starting amount of money in the pot</param>
        public void NotifyDealerAndPotAmount(Player dealer, int potAmount)
        {
            concreteClient.NotifyDealerAndPotAmount(dealer, potAmount);
        }

        /// <summary>
        /// Called by the client when a game is canceled by the server
        /// </summary>
        public void NotifyGameCanceled()
        {
            concreteClient.NotifyGameCanceled();
        }

        /// <summary>
        /// Called by the client when a game is already in progress. The server can't be connected
        /// </summary>
        public void NotifyGameInProgress()
        {
            canConnect = false;
            concreteClient.NotifyGameInProgress();
        }

        /// <summary>
        /// Called by the client when the name returned by <see cref="IClientHelper.GetName"/> already exists on the server.
        /// </summary>
        /// <remarks>
        /// Derived classes may use this method to create a new login name.
        /// </remarks>
        public void NotifyNameExists()
        {
            concreteClient.NotifyNameExists();
        }

        /// <summary>
        /// Called by the client when a new player is connected.
        /// </summary>
        /// <param name="player">The new player which was connected</param>
        public void NotifyNewUserConnected(Player player)
        {
            concreteClient.NotifyNewUserConnected(player);
        }


        /// <summary>
        /// Called by the client when a player disconnects
        /// </summary>
        /// <param name="player">The player which was disconnected</param>
        public void NotifyUserDisconnected(Player player)
        {
            concreteClient.NotifyUserDisconnected(player);
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
            checkBettingStarted();
            concreteClient.NotifyPlayerAction(player, betAction, callAmount, raiseAmount);
        }

        /// <summary>
        /// Called when a player is thinking of a move
        /// </summary>
        /// <param name="thinkingPlayer">The player which is thinking</param>
        public void NotifyPlayerIsThinking(Player thinkingPlayer)
        {
            concreteClient.NotifyPlayerIsThinking(thinkingPlayer);
        }

        /// <summary>
        /// Checks if need to call the <see cref="IClientHelper.NotifyBetRoundStarted"/>
        /// </summary>
        private void checkBettingStarted()
        {
            // test to see if need to call
            if (betRoundStarting)
            {
                // make the call
                concreteClient.NotifyBetRoundStarted();
                // mark as called
                betRoundStarting = false;
            }
        }

        /// <summary>
        /// Called by the clien when a player wins/loses.
        /// </summary>
        /// <param name="player">The player which won/lost</param>
        /// <param name="isWinner">True - the player won. False - The player lost.
        /// </param>
        public void NotifyPlayerStatus(Player player, bool isWinner)
        {
            checkResultIncomingStarted();
            concreteClient.NotifyPlayerStatus(player, isWinner);
        }

        /// <summary>
        /// Checks if need to call the <see cref="IClientHelper.NotifyResultsIncomingStarted"/>
        /// </summary>
        private void checkResultIncomingStarted()
        {
            // test to see if need to call
            if (notifyResultIncomingStarted)
            {
                // make the call
                concreteClient.NotifyResultsIncomingStarted();
                // mark as called
                notifyResultIncomingStarted = false;
            }
        }

        /// <summary>
        /// Called by the client after all of the results arrived
        /// </summary>
        public void NotifyResultsIncomingCompleted()
        {
            notifyResultIncomingStarted = true;
            concreteClient.NotifyResultsIncomingCompleted();
        }

        /// <summary>
        /// Called by the client when the current client can't participate in the current round.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifyStartingGame"/> is called prior to the round start.
        /// </remarks>
        public void NotifySittingOut()
        {
            concreteClient.NotifySittingOut();
        }

        /// <summary>
        /// Called by the client when the current round starts and the client is participating.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifySittingOut"/> is called prior to the round start.
        /// </remarks>
        public void NotifyStartingGame()
        {
            concreteClient.NotifyStartingGame();
        }

        /// <summary>
        /// Called by the client when each round ends. 
        /// </summary>
        /// <param name="gameOver">A flag which indicates the game is over.</param>
        /// <param name="winner">The winning player (if any), may be null</param>
        public void WaitOnRoundEnd(bool gameOver, Player winner)
        {
            concreteClient.WaitOnRoundEnd(gameOver, winner);
            if (gameOver)
                Running = false;
        }

        /// <summary>
        /// Called by the client to get the current player betting action. Derived classes must implement it and respond with a 
        /// proper action
        /// </summary>
        /// <param name="player">The player which needs to bet</param>
        /// <param name="action">The action to use to pass the player response</param>
        /// <returns>
        /// The player action modified to reflect the player action taken.
        /// </returns>
        public PlayerBettingAction WaitPlayerBettingAction(Player player, PlayerBettingAction action)
        {
            checkBettingStarted();
            concreteClient.WaitPlayerBettingAction(player, action);
            return action;
        }

        /// <summary>
        /// Called by the client when an update message arrives.
        /// </summary>
        /// <param name="player">The players sorted by their round order</param>
        /// <param name="potInformation">The current state of the pot</param>
        /// <param name="communityCards">The game community cards, if any. Can be null</param>
        public void WaitSynchronization(IEnumerable<Player> player, PotInformation potInformation, Card[] communityCards)
        {
            concreteClient.WaitSynchronization(player, potInformation, communityCards);
        }

        #endregion

        #region IPokerClient Members


        /// <summary>
        /// A method which is used to get the player drawing action. Derived classes must override this method to perform the action.
        /// </summary>
        /// <param name="player">The player which may draw new cards</param>
        /// <param name="action">The drawing action which must be updated with the player drawing selection</param>
        public PlayerDrawingAction WaitPlayerDrawingAction(Player player, PlayerDrawingAction action)
        {
            checkDrawingStarted();
            fiveCardConcreteClient.WaitPlayerDrawingAction(player, action);
            return action;
        }

        /// <summary>
        /// Checks if need to call the <see cref="IFiveCardClientHelper.NotifyDrawingRoundStarted"/>
        /// </summary>
        private void checkDrawingStarted()
        {
            // test to see if need to call
            if (drawingRoundStarted)
            {
                // make the call
                fiveCardConcreteClient.NotifyDrawingRoundStarted();
                // mark as called
                drawingRoundStarted = false;
            }
        }

        /// <summary>
        /// Called by the engine to notify the player with the new cards
        /// </summary>
        /// <param name="curPlayer">The player with the new drawn cards</param>
        public void NotifyPlayerNewCards(Player curPlayer)
        {
            fiveCardConcreteClient.NotifyPlayerNewCards(curPlayer);
        }

        /// <summary>
        /// Called by the engine after the drawing round completes.
        /// </summary>
        public void NotifyDrawComplete()
        {
            drawingRoundStarted = true;
            fiveCardConcreteClient.NotifyDrawingRoundCompleted();
        }
        /// <summary>
        /// Called by the client to notify how many cards were drawn by the player
        /// </summary>
        /// <param name="player">The drawing players</param>
        /// <param name="drawCount">The amount of cards drawn. Can be 0</param>
        public void NotifyPlayerDraws(Player player, int drawCount)
        {
            checkDrawingStarted();
            fiveCardConcreteClient.NotifyPlayerDraws(player, drawCount);
        }

        /// <summary>
        /// Called by the server to notify clients of server shutdown
        /// </summary>
        public void Bye()
        {
            Disconnect();
        }
        #endregion

        /// <summary>
        /// The proxy implementation which contacts the server side poker host
        /// </summary>
        private class PokerHostProxy : ClientBase<IPokerHost>, IPokerHost
        {
            public PokerHostProxy(Binding binding, EndpointAddress remoteAddress)
                : base(binding, remoteAddress)
            {
            }

            #region IPokerHost Members

            public ServerDetails GetServerDetails()
            {
                return Channel.GetServerDetails();
            }

            public IEnumerable<Player> GetLoggedinPlayers()
            {
                return Channel.GetLoggedinPlayers();
            }

            #endregion


        }

        /// <summary>
        /// The proxy implementation which contacts the server side poker service
        /// </summary>
        private class PokerServiceProxy : DuplexClientBase<IPokerService>, IPokerService
        {
            public PokerServiceProxy(InstanceContext callbackContext, Binding binding, EndpointAddress remoteAddress)
                : base(callbackContext, binding, remoteAddress)
            {
            }

            #region IPokerService Members

            public Player Login(string userName)
            {
                return Channel.Login(userName);
            }

            public void Logout()
            {
                Channel.Logout();
            }
            public TimeSpan GetCurrentActionDeadLine()
            {
                return Channel.GetCurrentActionDeadLine();
            }

            #endregion


        }

        /// <summary>
        /// The proxy implementation which contacts the server side broadcast service
        /// </summary>
        private class PokerBroadcastProxy : DuplexClientBase<IPokerServiceBroadcast>, IPokerServiceBroadcast
        {
            public PokerBroadcastProxy(InstanceContext callbackContext, Binding binding, EndpointAddress remoteAddress)
                : base(callbackContext, binding, remoteAddress)
            {
            }

            #region IPokerServiceBroadcast Members

            public bool RequestSpectation()
            {
                return Channel.RequestSpectation();
            }

            #endregion
        }

        /// <summary>
        /// The proxy implementation which contacts the server side chat room
        /// </summary>
        private class PokerRoomChatProxy : DuplexClientBase<IPokerRoomChat>, IPokerRoomChat
        {
            public PokerRoomChatProxy(InstanceContext callbackContext, Binding binding, EndpointAddress remoteAddress)
                : base(callbackContext, binding, remoteAddress)
            {

            }

            #region IPokerRoomChat Members

            public bool Login(string pokerServiceSessionId)
            {
                return Channel.Login(pokerServiceSessionId);
            }

            public void Logout()
            {
                Channel.Logout();
            }

            public void Speak(string message)
            {
                Channel.Speak(message);
            }

            #endregion
        }

        /// <summary>
        /// The callback instance to the chat room service, it notifies the client helper 
        /// when users talk
        /// </summary>
        private class PokerChatClient : IPokerChatClient
        {
            private IClientHelper helper;
            public PokerChatClient(IClientHelper helper)
            {
                this.helper = helper;
            }
            #region IPokerChatClient Members

            public void UserTalked(Player user, string message)
            {
                helper.UserTalked(user, message);
            }

            #endregion
        }

        #region IDisposable Members

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Disposes of the waithandle.
        /// </summary>
        /// <param name="disposing">A parameter which indicates if the dipose was called using the Dispose method</param>
        private void Dispose(bool disposing)
        {
            if (disposing && waitHandle != null)
            {
                waitHandle.Close();
                waitHandle = null;
            }
        }

        /// <summary>
        /// Part of the <see cref="IDisposable"/> implementation
        /// </summary>
        ~BaseWcfClient()
        {
            Dispose(false);
        }






    }
}
