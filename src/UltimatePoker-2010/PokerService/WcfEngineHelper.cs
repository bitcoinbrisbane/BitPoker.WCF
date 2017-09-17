using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using PokerEngine.Engine;
using PokerEngine;
using PokerRules.Games;
using BitPoker.Models.Deck;
using System.Threading;
using System.ServiceModel.Channels;

namespace PokerService
{
    /// <summary>
    /// The concrete implementation of the <see cref="IPokerService"/>, <see cref="IPokerHost"/> and the
    /// <see cref="IPokerServiceBroadcast"/> services. It is hosted by a <see cref="WcfEngineHost"/> and used by the <see cref="BaseEngine"/> 
    /// to communicate with the clients
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    public class WcfEngineHelper : IEngineHelper, IFiveCardEngineHelper, IPokerService, IPokerHost, IPokerServiceBroadcast
    {
        // the owining engine
        private BaseEngine owner;
        // the runing game
        private ServerGame game;
        // flags which indicates if the run started, if the cards will be revealed
        private bool hasRunStarted = false, revealCards = false;
        // the list of spectators (shared between services, must be locked)
        private List<IPokerClient> anonymousClients = new List<IPokerClient>();
        // a mapping between players to clients
        private Dictionary<Player, IPokerClient> playerToClient = new Dictionary<Player, IPokerClient>();
        // a mapping between the client session id to the associated player
        private Dictionary<string, Player> sessionIdToPlayer = new Dictionary<string, Player>();
        // a delegate which is used to copy players
        private Action<Player, Player> setSafePlayerCardsDelegate = defaultCopy;
        // a temporary list which holds newly connected player. The list is only filled when the engine supports new clients
        // newly connected players are added to the game on each round end.
        private List<Player> newlyConnectedPlayers = new List<Player>();
        // a list of players which lost
        private List<Player> losers = new List<Player>();
        // disconnected players are removed from the running engine.
        private HashSet<Player> disconnected = new HashSet<Player>();
        // a counter for the current hand
        private int currentHand = 0;
        // the current active player which has a deadline
        private Player curPlayerActive;
        // the time in which the current player deadline expires
        private DateTime currentPlayerDeadline;

        /// <summary>
        /// Replaces the player cards with empty cards.
        /// </summary>
        /// <param name="safe">The safe player to update</param>
        /// <param name="original">The original player to copy from</param>
        private static void defaultCopy(Player safe, Player original)
        {
            foreach (Card card in original.Cards)
                safe.Cards.Add(Card.Empty);
        }

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="WcfEngineHelper"/> class.</para>
        /// </summary>
        public WcfEngineHelper()
        {
        }

        /// <summary>
        /// Performs the given action on each of the clients (including spectators)
        /// </summary>
        /// <param name="action">The action to perform</param>
        private void forEachClient(Action<IPokerClient> action)
        {
            Player[] copy = playerToClient.Keys.ToArray();
            foreach (Player player in copy)
            {
                callAction(player, action);
            }
            foreachSpectator(action);
        }

        /// <summary>
        /// Performs an action on each of the spectators
        /// </summary>
        /// <param name="action">The action to perform</param>
        private void foreachSpectator(Action<IPokerClient> action)
        {
            // copy the clients
            IPokerClient[] clients = null;
            lock (anonymousClients)
            {
                clients = anonymousClients.ToArray();
            }
            // a temporary queue of malfunctioned clients
            Queue<IPokerClient> toRemove = new Queue<IPokerClient>();
            if (clients.Length > 0)
            {
                foreach (IPokerClient cur in clients)
                {
                    // call the action
                    try
                    {
                        action(cur);
                    }
                    catch
                    {
                        // in case of an error, mark as faulting, and abort
                        toRemove.Enqueue(cur);
                        try
                        {
                            ((ICommunicationObject)cur).Abort();
                        }
                        catch
                        {
                        }
                    }
                }
            }
            // remove all dropped listeners
            lock (anonymousClients)
            {
                while (toRemove.Count > 0)
                {
                    anonymousClients.Remove(toRemove.Dequeue());
                }
            }
        }

        /// <summary>
        /// Calls the given action on the player associated client
        /// </summary>
        /// <param name="player">The player which on which the action will be taken</param>
        /// <param name="action">The action to perform</param>
        private void callAction(Player player, Action<IPokerClient> action)
        {
            try
            {
                action(playerToClient[player]);
            }
            catch (TimeoutException)
            {
                // drop the player if has a timeout
                dropPlayer(player);
            }
            catch (Exception)
            {
                // mark the player as disconnected without closing the client.
                disconnected.Add(player);
            }
        }

        /// <summary>
        /// Makrs the player as disconnected and closes the connection
        /// </summary>
        /// <param name="player">The player to remove</param>
        private void dropPlayer(Player player)
        {
            disconnected.Add(player);
            ICommunicationObject comObject = playerToClient[player] as ICommunicationObject;
            if (comObject != null && comObject.State != CommunicationState.Closed)
            {
                comObject.Abort();
            }
        }

        /// <summary>
        /// Performs an action on each player except the given one (including spectators)
        /// </summary>
        /// <param name="excludedPlayer">The player which won't receive the action</param>
        /// <param name="action">The action to perform</param>
        private void forEachBut(Player excludedPlayer, Action<IPokerClient> action)
        {
            KeyValuePair<Player, IPokerClient>[] copy = playerToClient.ToArray();
            foreach (KeyValuePair<Player, IPokerClient> pair in copy)
            {
                if (pair.Key != excludedPlayer)
                {
                    callAction(pair.Key, action);
                }
            }
            foreachSpectator(action);
        }

        /// <summary>
        /// Calls a method on the client which has a response
        /// </summary>
        /// <typeparam name="T">The type of the result of the method</typeparam>
        /// <param name="player">The player to wait for a method</param>
        /// <param name="func">The method to perform</param>
        /// <returns>
        /// The result of the call or default of <typeparamref name="T"/> in case of an error or timeout.
        /// </returns>
        private T waitAction<T>(Player player, Func<IPokerClient, T> func)
        {
            T concreteResult = default(T);
            IContextChannel channel = (IContextChannel)playerToClient[player];
            TimeSpan oldTimeout = channel.OperationTimeout;
            try
            {
                // handle a call with a timeout
                if (owner.ActionTimeout > 0)
                {
                    // calculate the deadline
                    TimeSpan waitTime = TimeSpan.FromSeconds(owner.ActionTimeout);
                    currentPlayerDeadline = DateTime.Now + waitTime + TimeSpan.FromSeconds(1);
                    // set the new timeout
                    channel.OperationTimeout = waitTime + TimeSpan.FromSeconds(3);
                    // mark the player as the current waiting player
                    curPlayerActive = player;
                }                   
                else // handle call with no timeout
                {
                    // clear the current active player
                    curPlayerActive = null;
                    // set the maximal timeout with a spare time
                    channel.OperationTimeout = TimeSpan.FromSeconds(BaseEngine.MAXIMAL_TIMEOUT + 10);
                    
                }
                // call the action
                concreteResult = func(playerToClient[player]);

            }
            catch (TimeoutException)
            {
                // drop the player in case of a timeout
                dropPlayer(player);
            }
            catch (Exception)
            {
                // just mark the player as disconnected
                disconnected.Add(player);
            }
            finally
            {
                // return the low timeout
                channel.OperationTimeout = oldTimeout;
                curPlayerActive = null;
            }
            return concreteResult;
        }

        /// <summary>
        /// An event which is fired when a new player is connected
        /// </summary>
        public event DataEventHandler<Player> PlayerConnected;

        #region IPokerService Members


        /// <summary>
        /// Tries to login the client with the given user name.
        /// </summary>
        /// <param name="userName">The name to sign in the client</param>
        /// <returns>
        /// A valid player when the sign in succeeds. Null value is returned in case of an error (after a proper callback)
        /// </returns>
        /// <remarks>
        /// Either <see cref="IPokerClient.NotifyNameExists"/> or <see cref="IPokerClient.NotifyGameInProgress"/> is called in case 
        /// of an error.
        /// </remarks>
        public Player Login(string userName)
        {
            IPokerClient curClient = GetCurrentClient();
            // don't accept players with an existing name
            if (hasName(userName))
            {
                curClient.NotifyNameExists();
                return null;
            }
            // don't accept player after the game starts unless the engine expects them
            else if (hasRunStarted && !owner.AcceptPlayersAfterGameStart)
            {
                curClient.NotifyGameInProgress();
                return null;
            }
            else // the player can login
            {
                // create the player 
                Player result = new Player(userName);
                result.Money = owner.StartingMoney;
                // store the player in the dictionaries
                playerToClient.Add(result, curClient);
                sessionIdToPlayer.Add(OperationContext.Current.SessionId, result);
                // notify the clients that the new player is connected
                forEachClient((cur) => cur.NotifyNewUserConnected(result));
                // five the player connected event
                if (PlayerConnected != null)
                    PlayerConnected(this, new DataEventArgs<Player>(result));
                // add the players to the newlyConnectedPlayers in case that the engine has started
                if (hasRunStarted)
                    newlyConnectedPlayers.Add(result);
                return result;
            }
        }

        /// <summary>
        /// Gets the current client which invoked the operation
        /// </summary>
        /// <returns></returns>
        private IPokerClient GetCurrentClient()
        {
            return OperationContext.Current.GetCallbackChannel<IPokerClient>();
        }

        /// <summary>
        /// Request the server to logout from the current game
        /// </summary>
        public void Logout()
        {
            Player player = GetCurrentPlayer();
            playerToClient.Remove(player);
            sessionIdToPlayer.Remove(OperationContext.Current.SessionId);

            Player safePlayer = GetSafePlayer(player);
            forEachClient((cur) => cur.NotifyUserDisconnected(safePlayer));
        }

        /// <summary>
        /// Gets the current player associated with the client which invoked the operation
        /// </summary>
        /// <returns>The current player or null if none is associated</returns>
        private Player GetCurrentPlayer()
        {
            return GetCurrentPlayer(OperationContext.Current.SessionId);
        }

        /// <summary>
        /// Gets the current player associated with the given session id
        /// </summary>
        /// <param name="sessionId">The session id to look for</param>
        /// <returns>
        /// The associated player or null if none exists
        /// </returns>
        private Player GetCurrentPlayer(string sessionId)
        {
            Player result = null;
            if (!string.IsNullOrEmpty(sessionId))
                sessionIdToPlayer.TryGetValue(sessionId, out result);
            return result;
        }


        /// <summary>
        /// Checks if the given name is registered to one of the players
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private bool hasName(string userName)
        {
            foreach (Player player in playerToClient.Keys)
            {
                if (player.Name == userName)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Gets the player current action deadline. MUST be called by the player which needs to make a move
        /// </summary>
        /// <returns>
        /// A time span which indicates the remaining time for the player action, or <see cref="TimeSpan.Zero"/></returns>
        /// <remarks>
        /// When there is no deadline or when it isn't the calling client turn, a value of <see cref="TimeSpan.Zero"/> is returned.
        /// The client must repond with in the given time span.
        /// </remarks>
        public TimeSpan GetCurrentActionDeadLine()
        {
            // test to see if there is a deadline and if it is related to the calling player
            if (curPlayerActive == null || curPlayerActive != GetCurrentPlayer())
                return TimeSpan.Zero;
            // calculate the remaining time span (deduct a second owing to communication overhead)
            return currentPlayerDeadline - TimeSpan.FromSeconds(1) - DateTime.Now;
        }

        #endregion

        /// <summary>
        /// Initializes the helper with the required parameters
        /// </summary>
        /// <param name="engine">The engine which runs the game</param>
        /// <param name="game">The running game</param>
        public void Initialize(BaseEngine engine, ServerGame game)
        {
            this.owner = engine;
            this.game = game;
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
            this.currentHand = currentHand;
            forEachClient((cur) => cur.NotifyCurrentHand(currentHand));
        }

        /// <summary>
        /// Called when the game round starts.
        /// </summary>
        /// <param name="potAmount">The initial pot amount</param>
        /// <param name="dealer">The current round dealer</param>
        public void NotifyAntesAndDealer(int potAmount, Player dealer)
        {
            KeyValuePair<Player, IPokerClient>[] copy = playerToClient.ToArray();
            foreach (KeyValuePair<Player, IPokerClient> pair in copy)
            {
                // for each player, notify appropriate game start 
                if (losers.Contains(pair.Key))
                    callAction(pair.Key, (cur) => cur.NotifySittingOut());
                else
                    callAction(pair.Key, (cur) => cur.NotifyStartingGame());
            }
            // spectators always sit out
            foreachSpectator((cur) => cur.NotifySittingOut());
            Player safeDealer = GetSafePlayer(dealer);
            // let every one know the dealer and pot amount
            forEachClient((cur) => cur.NotifyDealerAndPotAmount(safeDealer, potAmount));
        }

        /// <summary>
        /// Called when the game round starts and the blind open is made
        /// </summary>
        /// <param name="opener">The blind opener</param>
        /// <param name="openAmount">The open amount, can be 0</param>
        public void NotifyBlindOpen(Player opener, int openAmount)
        {
            Player safeOpener = GetSafePlayer(opener);
            forEachClient((cur) => cur.NotifyBlindOpen(safeOpener, openAmount));
        }

        /// <summary>
        /// Called when the game round starts and the blind raise is made
        /// </summary>
        /// <param name="raiser">The blind raiser</param>
        /// <param name="raiseAmount">The raise amount, can be 0</param>
        public void NotifyBlindRaise(Player raiser, int raiseAmount)
        {
            // ignore raise with 0 amount
            if (raiseAmount > 0)
            {
                Player safeRaiser = GetSafePlayer(raiser);
                forEachClient((cur) => cur.NotifyBlindRaise(safeRaiser, raiseAmount));
            }
        }

        /// <summary>
        /// Called when a player loses the game. The player has lost all of the money and can't continue in the game.
        /// </summary>
        /// <param name="player">The losing player</param>
        public void OnDeclareLoser(Player player)
        {
            // mark the player as loser
            losers.Add(player);
            // if the player is marked as disconnected, can remove it here instead of waiting for NotifyWinLoseComplete
            if (disconnected.Remove(player))
            {
                doRemovePlayer(player);
            }
            Player safeLoser = GetSafePlayer(player);
            // notify players of losing player
            forEachClient((cur) => cur.NotifyPlayerStatus(player, false));
        }

        /// <summary>
        /// removes the player and disconnects the client
        /// </summary>
        /// <param name="player">The player to drop</param>
        private void doRemovePlayer(Player player)
        {
            try
            {
                ICommunicationObject comObject = playerToClient[player] as ICommunicationObject;
                // remove the player 
                playerToClient.Remove(player);
                // close the client if it isn't closed
                if (comObject.State != CommunicationState.Closed)
                {
                    comObject.Close(TimeSpan.FromSeconds(1));
                }
            }
            catch (ObjectDisposedException)
            {
            }
            Player safePlayer = GetSafePlayer(player);
            // notify clients of the disconnecting player
            forEachClient((cur) => cur.NotifyUserDisconnected(safePlayer));
        }

        /// <summary>
        /// Called when a player wins a round. There can be more than one winner per round.
        /// </summary>
        /// <param name="player">The winning player</param>
        /// <param name="result">The winning player hand</param>
        public void OnDeclareWinner(Player player, GameResult result)
        {
            Player safeWinner = GetSafePlayer(player);
            forEachClient((cur) => cur.NotifyPlayerStatus(player, true));
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
            forEachClient((cur) => cur.WaitOnRoundEnd(false, null));
            return true;
        }

        /// <summary>
        /// Called by the engine to get the players for the game. Derived classes must override it and return a collection 
        /// of players.
        /// </summary>
        /// <returns>
        /// A non null enumerable of players (must have at least 2 players)
        /// </returns>
        public IEnumerable<Player> WaitForPlayers()
        {
            // mark the game as running
            hasRunStarted = true;
            // return a copy of the signed in players
            Player[] clone = new Player[playerToClient.Count];
            playerToClient.Keys.CopyTo(clone, 0);
            return clone;
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
        public void WaitGameOver(Player winner)
        {
            Player safeWinner = GetSafePlayer(winner);
            forEachClient((cur) => cur.WaitOnRoundEnd(true, winner));
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
        public void WaitPlayerBettingAction(Player player, PlayerBettingAction action)
        {
            Player safeCopy = GetSafePlayer(player);
            forEachBut(player, (cur) => cur.NotifyPlayerIsThinking(safeCopy));
            PlayerBettingAction result = waitAction<PlayerBettingAction>(player, (cur) => cur.WaitPlayerBettingAction(player, action));
            // if the request failed, fold the player
            if (result == null)
            {
                action.Fold();
            }
            else
            {
                // pass the response back to the action
                switch (result.Action)
                {
                    case BetAction.CheckOrCall: action.Call(); break;
                    case BetAction.Fold: action.Fold(); break;
                    case BetAction.Raise: action.Raise(result.RaiseAmount); break;
                }
            }
        }

        /// <summary>
        /// Called by the engine in various occasions to update the players information. 
        /// </summary>
        /// <param name="orderedPlayers">The players in their current round order</param>
        /// <param name="potAmount">The current pot amount</param>
        /// <param name="communityCards">The game community cards, if any. Can be null</param>
        public void WaitSynchronizePlayers(IEnumerable<Player> orderedPlayers, int potAmount, Card[] communityCards)
        {
            List<Player> safePlayers = new List<Player>();
            // showdown, send the actual players
            if (revealCards)
            {
                revealCards = false;
                safePlayers.AddRange(orderedPlayers);
            }
            else // create a the list of safe players
            {
                foreach (Player player in orderedPlayers)
                {
                    safePlayers.Add(GetSafePlayer(player));
                }
            }

            PotInformation information = new PotInformation(potAmount, owner.PlayersBettingData);
            KeyValuePair<Player, IPokerClient>[] copy = playerToClient.ToArray();
            // send for each player the list of safe players, replace the player safe copy with the player actual data
            foreach (KeyValuePair<Player, IPokerClient> pair in copy)
            {
                Player[] sentArray = safePlayers.ToArray();
                // search for the player entry in the safe array:
                int index = Array.FindIndex<Player>(sentArray,
                    delegate(Player safe)
                    {
                        return safe.Name == pair.Key.Name;
                    });
                if (index != -1) // if the player was found, replace it with the original value
                {
                    sentArray[index] = pair.Key;
                }
                callAction(pair.Key, (cur) => cur.WaitSynchronization(sentArray, information, communityCards));
            }
            foreachSpectator((cur) => cur.WaitSynchronization(safePlayers.ToArray(), information, communityCards));
        }

        /// <summary>
        /// Called when the class initializes by <see cref="BaseEngine.Initialize"/>. Override it to add post constructor logic.
        /// </summary>
        public void OnInitialize()
        {

        }

        /// <summary>
        /// Called when the game run has started. Override it to add logic of game initialization
        /// </summary>
        public void OnRunStarted()
        {

        }

        /// <summary>
        /// Called after a betting round has completed
        /// </summary>
        /// <param name="allFolded">A flag indicating if all of the players folded and there is a single winner</param>
        public void NotifyRaiseComplete(bool allFolded)
        {
            if (allFolded)
            {
                forEachClient((cur) => cur.NotifyAllFolded());
            }
            else
            {
                forEachClient((cur) => cur.NotifyBetRoundComplete());
            }
            forEachClient((cur) => cur.FinalizeBettingRound());
        }

        /// <summary>
        /// Called after each round with a queue of losing players, the queue can be modified.
        /// </summary>
        /// <param name="losers">The queue of losers.</param>
        /// <remarks>
        /// Derived classes may modify the queue to add/remove losers.
        /// </remarks>
        public void NotifyWinLoseComplete(Queue<Player> losers)
        {
            revealCards = owner.HasRoundPlayers;
            // marked disconnected players as losers:
            Player[] copy = disconnected.ToArray();
            disconnected.Clear();
            foreach (Player cur in copy)
            {
                losers.Enqueue(cur);
                doRemovePlayer(cur);
            }
            forEachClient((cur) => cur.NotifyResultsIncomingCompleted());
        }

        /// <summary>
        /// Called at the round end to allow dervied classes to add new players. 
        /// </summary>
        /// <param name="toAdd">The queue of players to add to new players.</param>
        /// <remarks>
        /// This method called only when the game isn't in a <see cref="BaseEngine.TournamentMode"/><note>Note that the newly added players must have their initial money initialized to a positive sum.</note></remarks>
        public void GetNewPlayersAtRoundEnd(Queue<Player> toAdd)
        {
            Player[] copy = newlyConnectedPlayers.ToArray();
            newlyConnectedPlayers.Clear();
            foreach (Player addition in copy)
            {
                toAdd.Enqueue(addition);
            }
        }

        /// <summary>
        /// Called when the game run has completed. Override it to add cleanup logic
        /// </summary>
        public void OnRunComplete()
        {
            // close connections to each of the clients
            IPokerClient[] clients = playerToClient.Values.ToArray();
            foreach (IPokerClient pokerClient in clients)
            {
                ICommunicationObject client = (ICommunicationObject)pokerClient;
                try
                {
                    // ask the client to hang up..
                    pokerClient.Bye();
                    // wait for the client to try and disconnect
                    Thread.Sleep(TimeSpan.FromSeconds(1));

                    client.Close(TimeSpan.FromSeconds(2));
                }
                catch (TimeoutException)
                {
                    client.Abort();
                }
                catch (Exception)
                {
                }
            }
            playerToClient.Clear();
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
        public Player GetSafePlayer(Player player)
        {
            // create the copy
            Player result = new Player(player.Name);
            // uses a callback delegate to update the player cards. The default callback is the following method.
            setSafePlayerCardsDelegate(result, player);
            // update the player money
            result.Money = player.Money;
            result.CurrentBet = player.CurrentBet;
            return result;
        }

        /// <summary>
        /// Called after each player performs an action
        /// </summary>
        /// <param name="player">The player which performed the action </param>
        /// <param name="betAction">The action performed</param>
        /// <param name="callAmount">The amount which the player had to call</param>
        /// <param name="raiseAmount">The amount which the player raised (if any)</param>
        public void NotifyPlayerAction(Player player, BetAction betAction, int callAmount, int raiseAmount)
        {
            Player safePlayer = GetSafePlayer(player);
            forEachClient((cur) => cur.NotifyPlayerAction(safePlayer, betAction, callAmount, raiseAmount));
        }



        /// <summary>
        /// Gets or sets the safe player callback. The callback must not be null.
        /// </summary>
        /// <remarks>
        /// Use this callback to change the way a <see cref="GetSafePlayer(Player)"/> returns a player. 
        /// The callback first argument is the safe copy which needs to be updated, the second argument is the original player.
        /// </remarks>
        public Action<Player, Player> SetSafePlayerCards
        {
            get { return setSafePlayerCardsDelegate; }
            set { setSafePlayerCardsDelegate = value; }
        }

        #endregion



        #region IFiveCardEngineHelper Members

        /// <summary>
        /// A method which is used to get the player drawing action. Derived classes must override this method to perform the action.
        /// </summary>
        /// <param name="player">The player which may draw new cards</param>
        /// <param name="action">The drawing action which must be updated with the player drawing selection</param>
        public void WaitPlayerDrawingAction(Player player, PlayerDrawingAction action)
        {
            Player safeCopy = GetSafePlayer(player);
            forEachBut(player, (cur) => cur.NotifyPlayerIsThinking(safeCopy));
            PlayerDrawingAction result = waitAction<PlayerDrawingAction>(player, (cur) => cur.WaitPlayerDrawingAction(player, action));
            // when the request failed, draw 0 cards
            if (result != null && result.DrawnCards != null)
            {
                action.DrawnCards.AddRange(result.DrawnCards.Distinct());
                forEachBut(player, (cur) => cur.NotifyPlayerDraws(safeCopy, action.DrawnCards.Count));
            }
        }

        /// <summary>
        /// Called by the engine to notify the player with the new cards
        /// </summary>
        /// <param name="curPlayer">The player with the new drawn cards</param>
        public void NotifyPlayerNewCards(Player curPlayer)
        {
            callAction(curPlayer, (cur) => cur.NotifyPlayerNewCards(curPlayer));
        }

        /// <summary>
        /// Called by the engine after the drawing round completes.
        /// </summary>
        public void NotifyDrawComplete()
        {
            forEachClient((cur) => cur.NotifyDrawComplete());
        }

        #endregion

        #region IPokerHost Members

        /// <summary>
        /// Gets the host details which runs the game
        /// </summary>
        /// <returns>
        /// The running game details
        /// </returns>
        public ServerDetails GetServerDetails()
        {
            ServerDetails result = new ServerDetails();
            result.CanConnect = !hasRunStarted || owner.AcceptPlayersAfterGameStart;
            result.Game = game;
            result.ConnectedPlayers = playerToClient.Count;
            result.CurrentHand = currentHand;
            return result;
        }


        /// <summary>
        /// Gets a list of players which are connected to the game
        /// </summary>
        /// <returns>A list of players which are connected to the game</returns>
        public IEnumerable<Player> GetLoggedinPlayers()
        {
            // create a safe copy of the players
            Player[] copy = playerToClient.Keys.ToArray();
            for (int i = 0; i < copy.Length; ++i)
                copy[i] = GetSafePlayer(copy[i]);
            return copy;
        }

        #endregion

        #region IPokerServiceBroadcast Members

        /// <summary>
        /// Requests permissions to spectate the running game
        /// </summary>
        /// <returns>
        /// True if the client can watch, false otherwise.
        /// </returns>
        public bool RequestSpectation()
        {
            IPokerClient client = GetCurrentClient();
            client.NotifySittingOut();
            lock (anonymousClients)
            {
                anonymousClients.Add(client);

            }
            return true;
        }

        #endregion
        /// <summary>
        /// Gets a safe copy of the player with the associated session id
        /// </summary>
        /// <param name="pokerServiceSessionId">The player session id</param>
        /// <returns>
        /// A safe copy of the player or null if no player has the given session id.
        /// </returns>
        internal Player GetSafePlayer(string pokerServiceSessionId)
        {
            return GetCurrentPlayer(pokerServiceSessionId);
        }

    }
}
