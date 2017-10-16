using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using PokerEngine.Engine;
using System.ServiceModel.Channels;
using System.Threading;
using PokerEngine;
using System.ServiceModel.Discovery;

namespace PokerService
{
    /// <summary>
    /// A WCF implementation of the <see cref="IEngineHelper"/> interface. It provides WCF based communication between the server and the clients.
    /// </summary>
    /// <remarks>
    /// This helper creates the inproc hosting capabilities for the <see cref="WcfEngineHelper"/> helper.
    /// </remarks>
    public class WcfEngineHost : IEngineHelper, IFiveCardEngineHelper
    {
        // markers for each specific host
        private const int POKER_HOST = 2;
        private const int POKER_BROADCAST = 1;
        private const int POKER_SERVICE = 0;
        private const int POKER_CHAT = 3;

        // an array of service hosts which this helper opens
        private ServiceHost[] hosts = new ServiceHost[4];
        // the concrete helper
        private WcfEngineHelper concreteHelper;
        // a wrapping host service
        private WcfPokerHost pokerHost;
        // a wrapping broadcast service
        private WcfPokerBroadcast pokerBroadcast;
        // a wrapping chat service
        private WcfPokerRoomChat pokerChat;
        // the wait handle which is used to communicate with the engine helper
        private WaitHandle waitHandle;
        // the port in which the server is listening
        private int port;


        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="WcfEngineHost"/> class.</para>
        /// </summary>
        public WcfEngineHost()
        {
            // create the helpers
            concreteHelper = new WcfEngineHelper();
            pokerHost = new WcfPokerHost(concreteHelper);
            pokerBroadcast = new WcfPokerBroadcast(concreteHelper);
            pokerChat = new WcfPokerRoomChat(concreteHelper);
        }

        /// <summary>
        /// Initializes the host with all of the required information. This method must be called prior to the call to
        /// <see cref="BaseEngine.Run"/> method
        /// </summary>
        /// <param name="owner">The owning engine which configures the environment</param>
        /// <param name="game">The running game</param>
        /// <param name="port">The port in which the server will listen</param>
        /// <param name="waitHandle">The wait handle which is used to communicate with the engine helper</param>
        public void Initialize(BaseEngine owner, ServerGame game, int port, WaitHandle waitHandle)
        {
            concreteHelper.Initialize(owner, game);
            this.port = port;
            this.waitHandle = waitHandle;
        }

        /// <summary>
        /// An event which is fired when a new player is connected
        /// </summary>
        public event DataEventHandler<Player> PlayerConnected
        {
            add { concreteHelper.PlayerConnected += value; }
            remove { concreteHelper.PlayerConnected -= value; }
        }

        #region IEngineHelper Members

        /// <summary>
        /// Called when the current hand is raised. 
        /// </summary>
        /// <param name="currentHand">
        /// The current hand which is played
        /// </param>
        public void NotifyCurrentHand(int currentHand)
        {
            concreteHelper.NotifyCurrentHand(currentHand);
        }

        /// <summary>
        /// Called when the game round starts.
        /// </summary>
        /// <param name="potAmount">The initial pot amount</param>
        /// <param name="dealer">The current round dealer</param>
        public void NotifyAntesAndDealer(int potAmount, PokerEngine.Player dealer)
        {
            concreteHelper.NotifyAntesAndDealer(potAmount, dealer);
        }

        /// <summary>
        /// Called when the game round starts and the blind open is made
        /// </summary>
        /// <param name="opener">The blind opener</param>
        /// <param name="openAmount">The open amount, can be 0</param>
        public void NotifyBlindOpen(PokerEngine.Player opener, int openAmount)
        {
            concreteHelper.NotifyBlindOpen(opener, openAmount);
        }

        /// <summary>
        /// Called when the game round starts and the blind raise is made
        /// </summary>
        /// <param name="raiser">The blind raiser</param>
        /// <param name="raiseAmount">The raise amount, can be 0</param>
        public void NotifyBlindRaise(PokerEngine.Player raiser, int raiseAmount)
        {
            concreteHelper.NotifyBlindRaise(raiser, raiseAmount);
        }

        /// <summary>
        /// Called when a player loses the game. The player has lost all of the money and can't continue in the game.
        /// </summary>
        /// <param name="player">The losing player</param>
        public void OnDeclareLoser(PokerEngine.Player player)
        {
            concreteHelper.OnDeclareLoser(player);
        }

        /// <summary>
        /// Called when a player wins a round. There can be more than one winner per round.
        /// </summary>
        /// <param name="player">The winning player</param>
        /// <param name="result">The winning player hand</param>
        public void OnDeclareWinner(PokerEngine.Player player, PokerRules.Games.GameResult result)
        {
            concreteHelper.OnDeclareWinner(player, result);
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
        public bool WaitForAnotherRound()
        {
            return concreteHelper.WaitForAnotherRound();
        }

        /// <summary>
        /// Called by the engine to get the players for the game. Derived classes must override it and return a collection 
        /// of players.
        /// </summary>
        /// <returns>
        /// A non null enumerable of players (must have at least 2 players)
        /// </returns>
        public IEnumerable<PokerEngine.Player> WaitForPlayers()
        {
            // wait for the registration to close
            waitHandle.WaitOne();
            return concreteHelper.WaitForPlayers();
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
        public void WaitGameOver(PokerEngine.Player winner)
        {
            concreteHelper.WaitGameOver(winner);
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
        public void WaitPlayerBettingAction(PokerEngine.Player player, PlayerBettingAction action)
        {
            concreteHelper.WaitPlayerBettingAction(player, action);
        }

        /// <summary>
        /// Called by the engine in various occasions to update the players information. 
        /// </summary>
        /// <param name="orderedPlayers">The players in their current round order</param>
        /// <param name="potAmount">The current pot amount</param>
        /// <param name="communityCards">The game community cards, if any. Can be null</param>
        public void WaitSynchronizePlayers(IEnumerable<PokerEngine.Player> orderedPlayers, int potAmount, BitPoker.Models.Deck.Card[] communityCards)
        {
            concreteHelper.WaitSynchronizePlayers(orderedPlayers, potAmount, communityCards);
        }

        /// <summary>
        /// Called when the class initializes by <see cref="BaseEngine.Initialize"/>. Override it to add post constructor logic.
        /// </summary>
        public void OnInitialize()
        {
            // get the local machine name
            string hostName = System.Net.Dns.GetHostName();
            // setup the services
            hosts[POKER_SERVICE] = new ServiceHost(concreteHelper);
            hosts[POKER_BROADCAST] = new ServiceHost(pokerBroadcast);
            hosts[POKER_HOST] = new ServiceHost(pokerHost);
            hosts[POKER_CHAT] = new ServiceHost(pokerChat);
#if DEBUG
            // in debug mode, we want to know of the hosts faults
            foreach (ServiceHost host in hosts)
                host.Faulted += new EventHandler(host_Faulted);
#endif
            // setup the poker service
            NetTcpBinding netTcp = new NetTcpBinding(SecurityMode.None, true);
            netTcp.ReliableSession.Ordered = true;
            netTcp.ReceiveTimeout = TimeSpan.FromMinutes(10);
            netTcp.SendTimeout = TimeSpan.FromSeconds(15);

            hosts[POKER_SERVICE].AddServiceEndpoint(typeof(IPokerService), netTcp, string.Format("net.tcp://{0}:{1}/PokerService", hostName, port));
            // setup the host service
            netTcp = new NetTcpBinding(SecurityMode.None);
            netTcp.SendTimeout = TimeSpan.FromSeconds(15);
            hosts[POKER_HOST].AddServiceEndpoint(typeof(IPokerHost), netTcp, string.Format("net.tcp://{0}:{1}/PokerHost", hostName, port));
            ServiceDiscoveryBehavior discoveryBehavior = new ServiceDiscoveryBehavior();
            hosts[POKER_HOST].Description.Behaviors.Add(discoveryBehavior);
            hosts[POKER_HOST].AddServiceEndpoint(new UdpDiscoveryEndpoint());
            discoveryBehavior.AnnouncementEndpoints.Add(new UdpAnnouncementEndpoint());

            // setup the broadcast service
            netTcp = new NetTcpBinding(SecurityMode.None, true);
            netTcp.ReliableSession.Ordered = true;
            netTcp.ReceiveTimeout = TimeSpan.FromMinutes(10);
            netTcp.SendTimeout = TimeSpan.FromSeconds(15);
            hosts[POKER_BROADCAST].AddServiceEndpoint(typeof(IPokerServiceBroadcast), netTcp, string.Format("net.tcp://{0}:{1}/PokerServiceBroadcast", hostName, port));

            // setup the chat service
            netTcp = new NetTcpBinding(SecurityMode.None);
            netTcp.SendTimeout = TimeSpan.FromSeconds(15);
            hosts[POKER_CHAT].AddServiceEndpoint(typeof(IPokerRoomChat), netTcp, string.Format("net.tcp://{0}:{1}/PokerRoomChat", hostName, port));
            // open all hosts
            foreach (ServiceHost host in hosts)
                host.Open();

            concreteHelper.OnInitialize();
        }
#if DEBUG
        void host_Faulted(object sender, EventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
        }
#endif

        /// <summary>
        /// Called when the game run has started. Override it to add logic of game initialization
        /// </summary>
        public void OnRunStarted()
        {
            concreteHelper.OnRunStarted();
        }

        /// <summary>
        /// Called after a betting round has completed
        /// </summary>
        /// <param name="allFolded">A flag indicating if all of the players folded and there is a single winner</param>
        public void NotifyRaiseComplete(bool allFolded)
        {
            concreteHelper.NotifyRaiseComplete(allFolded);
        }

        /// <summary>
        /// Called after each round with a queue of losing players, the queue can be modified.
        /// </summary>
        /// <param name="losers">The queue of losers.</param>
        /// <remarks>
        /// Derived classes may modify the queue to add/remove losers.
        /// </remarks>
        public void NotifyWinLoseComplete(Queue<PokerEngine.Player> losers)
        {
            concreteHelper.NotifyWinLoseComplete(losers);
        }

        /// <summary>
        /// Called at the round end to allow dervied classes to add new players. 
        /// </summary>
        /// <param name="toAdd">The queue of players to add to new players.</param>
        /// <remarks>
        /// This method called only when the game isn't in a <see cref="BaseEngine.TournamentMode"/><note>Note that the newly added players must have their initial money initialized to a positive sum.</note></remarks>
        public void GetNewPlayersAtRoundEnd(Queue<PokerEngine.Player> toAdd)
        {
            concreteHelper.GetNewPlayersAtRoundEnd(toAdd);
        }

        /// <summary>
        /// Called when the game run has completed. Override it to add cleanup logic
        /// </summary>
        public void OnRunComplete()
        {
            // wait for a confirmation to close the server
            waitHandle.WaitOne();
            // in any case, wait for client to receive last messages
            Thread.Sleep(TimeSpan.FromSeconds(3));
            // let the helper cleanup
            concreteHelper.OnRunComplete();
            // close all hosts
            foreach (ServiceHost host in hosts)
            {
                try
                {
                    host.Close(TimeSpan.FromSeconds(2));
                }
                catch (TimeoutException)
                {
                    host.Abort();
                }
            }
        }

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
        public PokerEngine.Player GetSafePlayer(PokerEngine.Player player)
        {
            return concreteHelper.GetSafePlayer(player);
        }

        /// <summary>
        /// Called after each player performs an action
        /// </summary>
        /// <param name="player">The player which performed the action </param>
        /// <param name="betAction">The action performed</param>
        /// <param name="callAmount">The amount which the player had to call</param>
        /// <param name="raiseAmount">The amount which the player raised (if any)</param>
        public void NotifyPlayerAction(PokerEngine.Player player, BetAction betAction, int callAmount, int raiseAmount)
        {
            concreteHelper.NotifyPlayerAction(player, betAction, callAmount, raiseAmount);
        }

        /// <summary>
        /// Gets or sets the safe player callback. The callback must not be null.
        /// </summary>
        /// <remarks>
        /// Use this callback to change the way a <see cref="GetSafePlayer"/> returns a player. 
        /// The callback first argument is the safe copy which needs to be updated, the second argument is the original player.
        /// </remarks>
        public Action<PokerEngine.Player, PokerEngine.Player> SetSafePlayerCards
        {
            get { return concreteHelper.SetSafePlayerCards; }
            set { concreteHelper.SetSafePlayerCards = value; }
        }

        #endregion

        #region IFiveCardEngineHelper Members

        /// <summary>
        /// A method which is used to get the player drawing action. Derived classes must override this method to perform the action.
        /// </summary>
        /// <param name="player">The player which may draw new cards</param>
        /// <param name="action">The drawing action which must be updated with the player drawing selection</param>
        public void WaitPlayerDrawingAction(PokerEngine.Player player, PlayerDrawingAction action)
        {
            concreteHelper.WaitPlayerDrawingAction(player, action);
        }

        /// <summary>
        /// Called by the engine to notify the player with the new cards
        /// </summary>
        /// <param name="curPlayer">The player with the new drawn cards</param>
        public void NotifyPlayerNewCards(PokerEngine.Player curPlayer)
        {
            concreteHelper.NotifyPlayerNewCards(curPlayer);
        }

        /// <summary>
        /// Called by the engine after the drawing round completes.
        /// </summary>
        public void NotifyDrawComplete()
        {
            concreteHelper.NotifyDrawComplete();
        }

        #endregion
    }
}
