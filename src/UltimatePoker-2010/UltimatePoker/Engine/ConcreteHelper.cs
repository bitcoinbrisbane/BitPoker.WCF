using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using PokerEngine;
using PokerEngine.Engine;
using BitPoker.Models.Deck;
using System.Collections.ObjectModel;
using BitPoker.Models.Hands;
using PokerConsole.Engine;
using System.Windows.Threading;
using PokerRules.Games;
using PokerService;
using System.Windows.Documents;
using System.Windows.Media;

namespace UltimatePoker.Engine
{
    /// <summary>
    /// The concrete helper which handles the MainClientWindow interactions
    /// </summary>
    public class ConcreteHelper : IGuiClientHelper
    {
        /// <summary>
        /// The number of times the timer limit is updated in a second
        /// </summary>
        private const int TIMER_INTERVAL = 4;


        // The players pre defined brushes
        private static SolidColorBrush[] ChatBrushes = new SolidColorBrush[] { 
            new SolidColorBrush(Color.FromRgb(0xFF,0xD7,0x00)), 
            new SolidColorBrush(Color.FromRgb(0x00,0x80,0xff)),
            new SolidColorBrush(Color.FromRgb(0xFF,0x68,0x00)),
            new SolidColorBrush(Color.FromRgb(0xF2,0x0F,0x0F)),
            new SolidColorBrush(Color.FromRgb(0xB4,0x04,0xFF)),
            new SolidColorBrush(Color.FromRgb(0x00,0x16,0xAD)),
            new SolidColorBrush(Color.FromRgb(0x77,0x00,0x00))};

        // The main window in which the game is played
        private MainClientWindow mainWindow;
        // the game board which assits in players/cards/bets
        private PokerGameBoard board;
        // A mapping between each player and the proper wrapper
        private Dictionary<Player, GuiPlayerWrapper> playerToWrapper;
        // The current round order updated in each round start
        private List<Player> roundOrder = new List<Player>();
        // The type of the server game
        private GuiGames serverGame;
        // The client which will communicate with this helper
        private IRulesInterpreter client;
        // the initial name of the user, the game title which will be displayed on top of the window
        private string name, gameTitle;
        // A timer which is used to delay the response to the GuiClientHelper, a timer which provides the user the server time limit
        private DispatcherTimer timer = new DispatcherTimer(), timeLimiter = new DispatcherTimer();
        // a flag which indicates the game is running, a flag which indicates the player is sitting out
        private bool running = false, sittingOut = false, needToPromptForName = false;
        // A bet action which will be used to communicate the player betting response to the server
        private PlayerBettingAction betAction;
        // A drawing action which will be used to communicate the player drawing response to the server
        private PlayerDrawingAction drawingAction;
        // A temporary list of winners which is updated on each round. It marks the winning players.
        private List<Player> winners = new List<Player>();
        // A flag which is turned on when the round is over. It is used to display the winners hands.
        private bool onRoundOver = false;
        // store the blind open so it will be clear how much did the blind raise waged
        private int blindOpen;

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="ConcreteHelper"/> class.</para>
        /// </summary>
        /// <param name="initialName">The initial user name which is the login name of the player
        /// </param>
        /// <param name="client">The client which runs the game, it is used to get the player hands
        /// </param>
        public ConcreteHelper(string initialName, IRulesInterpreter client)
        {
            this.name = initialName;
            // use a name comparer to find values
            playerToWrapper = new Dictionary<Player, GuiPlayerWrapper>(new NameComparer());
            // setup the timer, it will be started when Respond(TimeSpan) is used
            timer.Tick += new EventHandler(timer_Tick);
            // setup the time limit timer, it is update TIME_INTERVALE times in a second
            timeLimiter.Tick += new EventHandler(timeLimiter_Tick);
            timeLimiter.Interval = TimeSpan.FromMilliseconds(1000 / TIMER_INTERVAL);
            this.client = client;
        }


        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="ConcreteHelper"/> class.</para>
        /// </summary>
        /// <param name="client">The client which runs the game, it is used to get the player hands
        /// </param>
        /// <remarks>
        /// This overload assumes the <see cref="Application.Current.MainWindow"/> is a <see cref="MainClientWindow"/>
        /// and will recycle that window.
        /// </remarks>
        public ConcreteHelper(IRulesInterpreter client)
            : this(null, client)
        {
            MainClientWindow mainWindow = Application.Current.MainWindow as MainClientWindow;
            if (mainWindow == null)
            {
                throw new InvalidOperationException("Can't create a helper with no main client window present");
            }

            this.name = mainWindow.GameBoard.ThePlayer.Name;
            attachToWindow(mainWindow);
        }

        /// <summary>
        /// Attaches a <see cref="MainClientWindow"/> to this helper.
        /// </summary>
        /// <param name="mainWindow">The window which is attached</param>
        private void attachToWindow(MainClientWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            mainWindow.Closed += new EventHandler(mainWindow_Closed);
            mainWindow.Closing += new System.ComponentModel.CancelEventHandler(mainWindow_Closing);
            mainWindow.logout.Click += new RoutedEventHandler(logout_Click);
            mainWindow.chatBox.KeyUp += new System.Windows.Input.KeyEventHandler(chatBox_KeyUp);
            this.board = mainWindow.GameBoard;
            SetSingleHelper(mainWindow, this);
        }


        /// <summary>
        /// Detaches the helper from the main window.
        /// </summary>
        private void detachFromWindow()
        {
            mainWindow.Closed -= new EventHandler(mainWindow_Closed);
            mainWindow.Closing -= new System.ComponentModel.CancelEventHandler(mainWindow_Closing);
            mainWindow.logout.Click -= new RoutedEventHandler(logout_Click);
            mainWindow.chatBox.KeyUp -= new System.Windows.Input.KeyEventHandler(chatBox_KeyUp);
        }

        /// <summary>
        /// A callback for the chat box enter key press
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void chatBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                // raise the user chat message
                if (userChatDelegate != null)
                    userChatDelegate(this, new DataEventArgs<string>(mainWindow.chatBox.Text));
                // don't wait for server callback, post the message immidiatly
                doAddUserTalk(board.ThePlayer.ThePlayer, mainWindow.chatBox.Text);
                mainWindow.chatBox.Clear();
            }
        }


        /// <summary>
        /// A callback for the Logout menu item. It aborts the current game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void logout_Click(object sender, RoutedEventArgs e)
        {
            Running = false;
            OnGameAborted(false);
        }

        /// <summary>
        /// A callback which is called when the main window is closing. The user needs to verify the exit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (running)
            {
                if (MessageBox.Show("Are you sure you want to quit the game?", "Game in progress",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    e.Cancel = true;
            }
        }

        /// <summary>
        /// A callback which is called when the main window is closed. This will abort the game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mainWindow_Closed(object sender, EventArgs e)
        {
            if (running)
            {
                Running = false;
                OnGameAborted(true);
            }
        }

        /// <summary>
        /// Raises the <see cref="GameAborted"/> event
        /// </summary>
        private void OnGameAborted(bool terminating)
        {
            if (GameAborted != null)
            {
                GameAborted(this, new DataEventArgs<bool>(terminating));
            }
        }

        /// <summary>
        /// A callback for the timer tick. It will raise the <see cref="GuiClientResponded"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            OnGuiClientResponded();
        }

        /// <summary>
        /// Gets the board which is being played
        /// </summary>
        public PokerGameBoard Board { get { return board; } }

        /// <summary>
        /// Gets or sets the client which uses this helper
        /// </summary>
        public IRulesInterpreter Client
        {
            get { return client; }
            set { client = value; }
        }

        /// <summary>
        /// Responds with the <see cref="GuiClientResponded"/> after the specified amount of time.
        /// </summary>
        /// <param name="timeout">The time to wait before raising the event</param>
        internal void Respond(TimeSpan timeout)
        {
            // use the game speed to factor with the defined wait time.
            double newInterval = mainWindow.GameSpeed * timeout.TotalMilliseconds;

            // don't wait forever
            if (newInterval < 0)
                newInterval = 0;
            // start the timer with the specified interval
            timer.Interval = TimeSpan.FromMilliseconds(newInterval);
            timer.Start();

        }

        /// <summary>
        /// Raises the <see cref="GuiClientResponded"/> event
        /// </summary>
        protected void OnGuiClientResponded()
        {
            if (timeLimiter.IsEnabled)
            {
                timeLimiter.Stop();
                board.ThePlayer.RemainingTime = 0;
            }
            if (GuiClientResponded != null)
                GuiClientResponded(this, EventArgs.Empty);
        }

        #region IGuiClientHelper Members

        /// <summary>
        /// This event is raised when the clinet responds, the client should be ready getting new notifications.
        /// </summary>
        public event EventHandler GuiClientResponded;

        #endregion

        #region IClientHelper Members

        /// <summary>
        /// Called when the betting round finalizes in any case the round is over.
        /// </summary>
        public void FinalizeBettingRound()
        {
            // add a the handler for the timer tick to clear the board
            timer.Tick += new EventHandler(clearAfterBet);
            Respond(TimeSpan.FromMilliseconds(1250));
        }

        /// <summary>
        /// A callback for the timer which is used by <see cref="FinalizeBettingRound"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clearAfterBet(object sender, EventArgs e)
        {
            // remove the callback so it won't be called after the current tick.
            timer.Tick -= new EventHandler(clearAfterBet);
            board.ClearAllMessages();
        }

        /// <summary>
        /// Gets the login name of the client. This method may be called again after a call to <see cref="NotifyNameExists"/></summary>
        /// <returns>A valid user name to login the client</returns>
        /// <remarks>
        /// This method must aquire the user login name. A new value must be returned after a call to <see cref="NotifyNameExists"/></remarks>
        public string GetName()
        {
            if (needToPromptForName)
            {
                needToPromptForName = false;
                // use a dialog box to get a new name
                PromptNameWindow getName = new PromptNameWindow();
                // set the current name as the user name text value:
                getName.UserName.Text = name;
                if (getName.ShowDialog().Value)
                {
                    // the user approved the dialog, get the new name and respond
                    this.name = getName.UserName.Text;
                    OnGuiClientResponded();
                }
                else
                {
                    // the user canceled, abort the client
                    OnGameAborted(false);
                }
            }
            return name;
        }

        /// <summary>
        /// Called when the betting round is completed and there is a single winner.
        /// </summary>
        public void NotifyAllFolded()
        {
            board.PostExclusiveMessage(board.ThePlayer, "There is a winner");
            Respond(TimeSpan.FromMilliseconds(750));
        }

        /// <summary>
        /// Called when a betting round is completed and all players have checked.
        /// </summary>
        public void NotifyBetRoundComplete()
        {
            board.ThePlayer.Message = "No more bets";
            LogLine("No more bets\n");
            Respond(TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// Called before a betting round starts.
        /// </summary>
        public void NotifyBetRoundStarted()
        {
            board.ThePlayer.Message = "Waiting for bets";
            OnGuiClientResponded();
        }

        /// <summary>
        /// Called by the client when the blind open is made.
        /// </summary>
        /// <param name="opened">The player which blind opens</param>
        /// <param name="openAmount">The player open amount, may be 0</param>
        public void NotifyBlindOpen(Player opened, int openAmount)
        {
            this.blindOpen = openAmount;
            notifyBlind(opened, string.Format("blind opens with {0}$", openAmount), openAmount);
        }

        /// <summary>
        /// A message which displays the blind message of the given player.
        /// </summary>
        /// <param name="player">The player which performed the action</param>
        /// <param name="message">The message to show with the player</param>
        /// <param name="totalAmount">The amount which the player bet</param>
        private void notifyBlind(Player player, string message, int amount)
        {
            // don't display blind actions with an amount of 0
            if (amount > 0)
            {
                updateAndLogPlayer(player, message);
                Respond(TimeSpan.FromMilliseconds(750));
            }
            else
            {
                // no need to wait when not displaying the message.
                OnGuiClientResponded();
            }
        }

        /// <summary>
        /// Updates the player with the given message and the player money.
        /// </summary>
        /// <param name="player">The player to update</param>
        /// <param name="raiseMessage">The player new message</param>
        /// <remarks>
        /// This does not update the player cards. To update the player cards use <see cref="updatePlayerCards"/>
        /// </remarks>
        private void updatePlayer(Player player, string raiseMessage)
        {
            CheckHasPlayer(player);
            GuiPlayerWrapper wrapper = playerToWrapper[player];
            board.ActivatePlayer(wrapper);
            wrapper.Money = player.Money;
            wrapper.Message = raiseMessage;
        }

        /// <summary>
        /// Updates the player message and logs the message to the window log.
        /// </summary>
        /// <param name="player">The player to post the message to</param>
        /// <param name="message">The new message</param>
        private void updateAndLogPlayer(Player player, string message)
        {
            updatePlayer(player, message);
            LogLine(string.Format("{0} {1}", player.Name, message));

        }

        /// <summary>
        /// Logs a line of text to the main window log
        /// </summary>
        /// <param name="message">The message to add</param>
        public void LogLine(string message)
        {
            mainWindow.logContent.Inlines.Add(new System.Windows.Documents.Run(string.Format("{0}{1}", message, Environment.NewLine)));
            mainWindow.logBox.ScrollToEnd();
        }



        /// <summary>
        /// Called by the client when the blind raise is made.
        /// </summary>
        /// <param name="raiser">The player which blind raises</param>
        /// <param name="raiseAmount">The raise amound, may be 0</param>
        public void NotifyBlindRaise(Player raiser, int raiseAmount)
        {
            notifyBlind(raiser, string.Format("blind calls {0}$ & raises {1}", this.blindOpen, raiseAmount), blindOpen + raiseAmount);
        }

        /// <summary>
        /// Gets or sets the running state of the game.
        /// </summary>
        private bool Running
        {
            get { return running; }
            set
            {
                running = value;
                board.IsRunning = value;
                // fix the main window title when the running state changes
                if (running)
                {
                    mainWindow.Title = "Ultimate Poker - running";
                }
                else
                {
                    mainWindow.Title = "Ultimate Poker - game over";
                    board.BoardTitle = "Game Over";
                    board.ThePlayer.RemainingTime = 0;
                    timeLimiter.Stop();
                    timer.Stop();
                    detachFromWindow();

                }
            }
        }



        /// <summary>
        /// Called by the client when a connection is made successfuly to the server
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint which was opened locally.
        /// </param>
        public void NotifyConnectedToServer(System.Net.EndPoint endPoint)
        {
            OnGuiClientResponded();
        }

        /// <summary>
        /// Called by the client when the connection to the server is closed
        /// </summary>
        public void NotifyConnectionClosed()
        {
            updateAndLogPlayer(board.ThePlayer.ThePlayer, "Server Connection Closed");
            Running = false;
            OnGuiClientResponded();
        }

        /// <summary>
        /// Called by the client when a round starts.
        /// </summary>
        /// <param name="dealer">The current round dealer</param>
        /// <param name="potAmount">The starting amount of money in the pot</param>
        public void NotifyDealerAndPotAmount(Player dealer, int potAmount)
        {
            board.Pot = potAmount;
            // show this message for the dealer
            updateAndLogPlayer(dealer, "Is the dealer");
            foreach (GuiPlayerWrapper wrapper in playerToWrapper.Values)
                // mark the "IsDealer" property as true only at the dealer player
                wrapper.IsDealer = (dealer.Name == wrapper.Name);

            roundOrder.Clear();

            Respond(TimeSpan.FromMilliseconds(1500));
        }

        /// <summary>
        /// Called by the client when a game is canceled by the server
        /// </summary>
        public void NotifyGameCanceled()
        {
            MessageBox.Show("Game Canceled", "Game Over");
            Running = false;
            OnGuiClientResponded();
        }

        /// <summary>
        /// Called by the client when a game is already in progress. The server can't be connected
        /// </summary>
        public void NotifyGameInProgress()
        {
            MessageBox.Show("Game is already in progress. Try again later", "Server is busy", MessageBoxButton.OK);
            Running = false;
            OnGuiClientResponded();
        }

        /// <summary>
        /// Called by the client when the name returned by <see cref="GetName"/> already exists on the server.
        /// </summary>
        /// <remarks>
        /// Derived classes may use this method to create a new login name.
        /// </remarks>
        public void NotifyNameExists()
        {
            needToPromptForName = true;
            OnGuiClientResponded();
        }

        /// <summary>
        /// Called by the client when a new player is connected.
        /// </summary>
        /// <param name="player">The new player which was connected</param>
        public void NotifyNewUserConnected(Player player)
        {
            // The main window is initialized here since this is the earliest point whre the game starts.
            if (mainWindow == null)
            {
                // create a new window and register to the window events.
                mainWindow = new MainClientWindow();
                attachToWindow(mainWindow);
                // show the window and fix the title
                mainWindow.Show();
                Running = true;
                board.BoardTitle = "Ultimate Poker - " + gameTitle;
            }

            GuiPlayerWrapper addition = null;
            // a new player was connected, update the board:
            if (player.Name == this.name)
            {
                // create a new main player
                board.ThePlayer = new GuiMainPlayer(player, serverGame);
                addition = board.ThePlayer;
                // map the wrapper to the player
                playerToWrapper.Add(player, addition);
            }
            else
            {
                // add a regualr player
                addition = doAddPlayer(player);
            }
            addition.Message = "Connected";
            board.ActivatePlayer(addition);
            Respond(TimeSpan.FromMilliseconds(250));
        }

        /// <summary>
        /// Creates a new <see cref="GuiPlayerWrapper"/> for the player and maps it
        /// </summary>
        /// <param name="player">The player to map</param>
        /// <returns>The newly created <see cref="GuiPlayerWrapper"/></returns>
        private GuiPlayerWrapper doAddPlayer(Player player)
        {
            GuiPlayerWrapper addition = new GuiPlayerWrapper(player);
            addition.AssociatedColor = ChatBrushes[board.OtherPlayers.Count % ChatBrushes.Length];
            board.OtherPlayers.Add(addition);
            playerToWrapper.Add(player, addition);
            UpdatePlayerCards(addition, player.Cards);
            return addition;
        }


        /// <summary>
        /// A callback which manages the player actions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ThePlayer_PlayerPerformedAction(object sender, ActionEventArgs e)
        {
            bool respond = true;
            // don't accept anymore clicks
            board.ThePlayer.PlayerPerformedAction -= new EventHandler<ActionEventArgs>(ThePlayer_PlayerPerformedAction);
            // handle the action
            switch (e.Action)
            {
                case GuiActions.AllIn: betAction.Raise(board.ThePlayer.Money); break;
                case GuiActions.Bet: betAction.Raise(board.ThePlayer.CurrentBet); break;
                case GuiActions.Call:
                case GuiActions.Check: betAction.Call(); break;
                case GuiActions.Fold:
                    {
                        // assure the player confirms the fold when there is no bet on the table
                        if (betAction.CallAmount == 0)
                        {
                            respond = false;
                            // don't allow raise in the current state
                            disableAllActions();
                            board.ThePlayer.ChangeActionState(GuiActions.Check, true);
                            board.ThePlayer.ChangeActionState(GuiActions.Fold, true);
                            // in case that the timer is visible, make it transparent so the message will be seen
                            mainWindow.MainPlayer.timeLimit.Opacity = 0.2;
                            board.ThePlayer.Message = "There is no bet. Are you sure you want to fold?";
                            board.ThePlayer.PlayerPerformedAction += new EventHandler<ActionEventArgs>(confirmFold);
                        }
                        else
                            betAction.Fold();
                    } break;
            }
            // don't respond until the user confirms
            if (respond)
            {
                // send the respond back to the client
                OnGuiClientResponded();
            }
        }

        /// <summary>
        /// A callback which is used to confirm player fold 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void confirmFold(object sender, ActionEventArgs e)
        {
            // revert the timer opacity back
            mainWindow.MainPlayer.timeLimit.Opacity = 1;
            board.ThePlayer.PlayerPerformedAction -= confirmFold;
            // only check/fold are possible in this state
            switch (e.Action)
            {
                case GuiActions.Fold: betAction.Fold(); break;
                case GuiActions.Check: betAction.Call(); break;
            }
            OnGuiClientResponded();
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
            CheckHasPlayer(player);

            // disable the player actions on the board.
            disableAllActions();

            this.betAction = null;

            string message = string.Empty;
            int totalAmount = callAmount + raiseAmount;
            if (betAction == BetAction.Raise && player.Money == 0)
                // the player is all in
                message = string.Format("All In {0}$", totalAmount);
            else
            {
                // get the player message
                switch (betAction)
                {
                    case BetAction.CheckOrCall: // the player checekd
                        if (callAmount == 0)
                            message = "Checked";
                        else
                            message = string.Format("Called {0}$", callAmount);
                        break;
                    case BetAction.Raise: // the player raised
                        if (callAmount == 0)
                            message = string.Format("Raised {0}$", raiseAmount);
                        else
                            message = string.Format("Called {0}$ & Raised {1}$", callAmount, raiseAmount);
                        break;
                    case BetAction.Fold: message = "Folds"; // the player folds
                        {
                            playerToWrapper[player].Cards.Clear();
                        } break;
                }
            }
            if (betAction != BetAction.Fold)
            {
                // manualy increate the pot size, don't wait for the next synchronization.
                int potIncrease = playerToWrapper[player].Money - player.Money;
                board.Pot += potIncrease;
                playerToWrapper[player].PotInvestment = player.CurrentBet;
            }
            // update the player with the given message
            updateAndLogPlayer(player, message);
            Respond(TimeSpan.FromMilliseconds(1500));
        }

        /// <summary>
        /// Updates the player cards With the given cards
        /// </summary>
        /// <param name="wrapper">The player to update</param>
        /// <param name="cards">The player new cards</param>
        protected virtual void UpdatePlayerCards(GuiPlayerWrapper wrapper, IEnumerable<Card> cards)
        {
            doUpdatePlayerCards(wrapper, cards);
        }

        /// <summary>
        /// Updates the player cards With the given cards
        /// </summary>
        /// <param name="player">The player to update</param>
        /// <param name="cards">The player new cards</param>
        private void updatePlayerCards(Player player, IEnumerable<Card> cards)
        {
            GuiPlayerWrapper wrapper = playerToWrapper[player];
            UpdatePlayerCards(wrapper, cards);
        }

        /// <summary>
        /// The default implementation of the <see cref="UpdatePlayerCardsDelegate"/>
        /// </summary>
        /// <param name="wrapper">The wrapper to update</param>
        /// <param name="cards">The new player cards</param>
        private void doUpdatePlayerCards(GuiPlayerWrapper wrapper, IEnumerable<Card> cards)
        {
            Player originalPlayer = wrapper.ThePlayer;

            // clear the old player cards
            originalPlayer.Cards.Clear();
            if (cards != null)
            {
                // add the new cards
                foreach (Card card in cards)
                {
                    originalPlayer.Cards.Add(card);
                }
                // if a valid hand is found, update the hand and select the cards.
                Hand hand = client.GetBestHand(cards);
                if (hand != null)
                {
                    board.SelectExclusiveCards(new List<Card>(hand));
                    wrapper.CurrentHand = hand;
                }
            }
        }

        /// <summary>
        /// Called when a player is thinking of a move
        /// </summary>
        /// <param name="thinkingPlayer">The player which is thinking</param>
        public void NotifyPlayerIsThinking(Player thinkingPlayer)
        {
            updatePlayer(thinkingPlayer, "Thinking...");
            Respond(TimeSpan.FromMilliseconds(750));
        }

        /// <summary>
        /// Called by the clien when a player wins/loses.
        /// </summary>
        /// <param name="player">The player which won/lost</param>
        /// <param name="isWinner">True - the player won. False - The player lost.
        /// </param>
        public void NotifyPlayerStatus(Player player, bool isWinner)
        {
            if (isWinner)
            {
                // Add the player message and mark it as a winner.
                updateAndLogPlayer(player, "Wins");
                winners.Add(player);
            }
            else
            {
                // notify the losing player
                updateAndLogPlayer(player, "Can't pay for next round. LOSER!");
            }
            Respond(TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// Called by the client after all of the results arrived
        /// </summary>
        public void NotifyResultsIncomingCompleted()
        {
            // a list which will hold all of the winning cards.
            List<Card> clone = new List<Card>();
            // clear the board active players
            foreach (GuiPlayerWrapper wrapper in playerToWrapper.Values)
            {
                wrapper.IsActive = false;
            }

            foreach (Player winner in winners)
            {
                // mark the winner as active
                playerToWrapper[winner].IsActive = true;
                // set a win message
                playerToWrapper[winner].Message = "Wins";
                // If the player hand is found, add the hand cards to the cloned list
                Hand hand = client.GetBestHand(winner.Cards);
                if (hand != null)
                    clone.AddRange(hand);
            }
            // select all of the cards which the winners had
            board.SelectExclusiveCards(clone);
            Respond(TimeSpan.FromMilliseconds(1250));
        }

        /// <summary>
        /// Called by the client before the results starts to arrive.
        /// </summary>
        public void NotifyResultsIncomingStarted()
        {
            board.ClearAllMessages();
            // mark the round as over so in the next synchronization it will mark the winners.
            onRoundOver = true;
            // clear the previous round winners
            winners.Clear();
            OnGuiClientResponded();
        }

        /// <summary>
        /// Called by the client when a server identifies the running game.
        /// </summary>
        /// <param name="serverGame">The type of game the server is running</param>
        /// <param name="connectedPlayers">The number of players currently connected to the server</param>
        public void NotifyRunningGame(ServerGame serverGame, int connectedPlayers)
        {
            // select the game title
            switch (serverGame)
            {
                case ServerGame.FiveCardDraw:
                    this.serverGame = GuiGames.FiveCardDraw;
                    gameTitle = "Five Card Draw";
                    break;
                case ServerGame.TexasHoldem:
                    this.serverGame = GuiGames.TexasHoldem;
                    gameTitle = "Texas Hold'em";
                    break;
                case ServerGame.SevenCardStud: // TODO check if need 7 card stud mode
                    gameTitle = "Seven Card Stud";
                    this.serverGame = GuiGames.TexasHoldem;
                    break;
                case ServerGame.OmahaHoldem:
                    gameTitle = "Omaha Hold'em";
                    this.serverGame = GuiGames.TexasHoldem;
                    break;
            }
            // the window can be recycled, or craeted at NotifyUserCreated
            if (mainWindow != null)
            {
                board.OtherPlayers.Clear();
                clearBoard();
                Running = true;
                board.BoardTitle = "Ultimate Poker - " + gameTitle;
            }
            OnGuiClientResponded();
        }

        /// <summary>
        /// Called by the client when the current client can't participate in the current round.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifyStartingGame"/> is called prior to the round start.
        /// </remarks>
        public void NotifySittingOut()
        {
            // perform these actions only once:
            if (!sittingOut)
            {
                sittingOut = true;
                // set the game speed to 0.01 so the players won't wait for this client
                mainWindow.GameSpeed = 0.01;
            }
            clearBoard();
            board.ThePlayer.Message = "Sitting out";
            Respond(TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// Called by the client when the current round starts and the client is participating.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifySittingOut"/> is called prior to the round start.
        /// </remarks>
        public void NotifyStartingGame()
        {
            clearBoard();

            board.PostExclusiveMessage(board.ThePlayer, "Starting game");
            Respond(TimeSpan.FromMilliseconds(750));
        }

        /// <summary>
        /// Clears the game board.
        /// </summary>
        private void clearBoard()
        {
            board.CommunityCards.Clear();

            foreach (GuiPlayerWrapper wrapper in playerToWrapper.Values)
            {
                wrapper.Message = string.Empty;
                wrapper.Cards.Clear();
                wrapper.CurrentHand = null;
                wrapper.IsActive = false;
            }
            board.ClearAllCardsSelection();
            mainWindow.logContent.Inlines.Clear();
            disableAllActions();
        }
        /// <summary>
        /// Disable all of the player actions.
        /// </summary>
        protected void disableAllActions()
        {
            foreach (GuiActions tmpAction in disabledActions)
                board.ThePlayer.ChangeActionState(tmpAction, false);
        }

        /// <summary>
        /// Called by the client when each round ends. 
        /// </summary>
        /// <param name="gameOver">A flag which indicates the game is over.</param>
        /// <param name="winner">The winning player (if any), may be null</param>
        public void WaitOnRoundEnd(bool gameOver, Player winner)
        {
            // clear the round over flag.
            onRoundOver = false;
            if (gameOver)
            {
                // The game was canceled.
                if (winner == null)
                {
                    MessageBox.Show("Game was canceled", "Game Over");
                    Running = false;
                }
                else // keep the session open until the server closes the connection
                {
                    CheckHasPlayer(winner);
                    board.PostExclusiveMessage(playerToWrapper[winner], "Wins!");
                    board.BoardTitle = "Game Over";
                }
                OnGuiClientResponded();

            }
            else if (sittingOut)
            {
                // don't wait for player which lost
                Respond(TimeSpan.FromMilliseconds(300));
            }
            else
            {
                // set the player message
                string message = board.ThePlayer.Message;

                if (string.IsNullOrEmpty(message))
                    message = "Click to start next round";
                else
                    message += "- Click to start next round";

                board.ThePlayer.Message = message;
                // turn the Deal action to be the only available action
                disableAllActions();

                board.ThePlayer.ChangeActionState(GuiActions.Deal, true);

                board.ThePlayer.PlayerPerformedAction += new EventHandler<ActionEventArgs>(Continue);

                board.ThePlayer.CurrentBet = 0;
            }

        }

        /// <summary>
        /// A callback for the deal button click. It simply raises the client reponse.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Continue(object sender, ActionEventArgs e)
        {
            board.ThePlayer.PlayerPerformedAction -= new EventHandler<ActionEventArgs>(Continue);
            OnGuiClientResponded();
        }

        // The list of actions to disable when no action is needed
        private static GuiActions[] disabledActions = new GuiActions[] { GuiActions.Bet, GuiActions.Call, GuiActions.Check, GuiActions.Deal, GuiActions.Fold, GuiActions.AllIn, GuiActions.Draw, GuiActions.Logout };

        /// <summary>
        /// Called by the client to get the current player betting action. Derived classes must implement it and respond with a 
        /// proper action
        /// </summary>
        /// <param name="player">The player which needs to bet</param>
        /// <param name="action">The action to use to pass the player response</param>
        public void WaitPlayerBettingAction(Player player, PlayerBettingAction action)
        {
            // set the current bet action
            betAction = action;
            // register to the player action handling
            board.ThePlayer.PlayerPerformedAction += new EventHandler<ActionEventArgs>(ThePlayer_PlayerPerformedAction);
            // notify the player that an action is required
            updatePlayer(player, "Place your bet");
            updatePlayerCards(player, player.Cards);

            disableAllActions();
            // The player can go "all in"
            if (action.CanRaise || action.IsAllInMode)
                board.ThePlayer.ChangeActionState(GuiActions.AllIn, true);
            // The player can always fold
            board.ThePlayer.ChangeActionState(GuiActions.Fold, true);
            // The player can call
            if (action.CallAmount == 0)
            {
                board.ThePlayer.ChangeActionState(GuiActions.Check, true);
            }
            else if (!action.IsAllInMode)
            {
                // The player doesn't have to go "all in"
                board.ThePlayer.ChangeActionState(GuiActions.Call, true);
            }
            if (!action.IsAllInMode && action.CanRaise)
            {
                // The player can raise and doesn't have to go "all in"
                board.ThePlayer.ChangeActionState(GuiActions.Bet, true);
            }

            // update the player call sum and minimal bet
            board.ThePlayer.CallSum = action.CallAmount;
            board.ThePlayer.MinimalBet = action.RaiseAmount;
            // verify the current bet is over the minimal bet limit
            if (board.ThePlayer.ThePlayer.CurrentBet < action.RaiseAmount)
                board.ThePlayer.CurrentBet = action.RaiseAmount;
        }

        /// <summary>
        /// Called by the client when an update message arrives.
        /// </summary>
        /// <param name="players">The players sorted by their round order</param>
        /// <param name="information">The current state of the pot</param>
        /// <param name="communityCards">The community cards in the game (if any) may be null or in 0 length</param>
        public virtual void WaitSynchronization(IEnumerable<Player> players, PokerService.PotInformation information, Card[] communityCards)
        {
            // create the current round order
            if (roundOrder.Count == 0)
            {
                roundOrder.AddRange(players);
                assureBoardOrder();
            }
            // don't clear the pot information at the round end
            if (information.PotAmount != 0)
            {
                board.Pot = information.PotAmount;
                mainWindow.potInformation.UpdatePotInformation(roundOrder, information.SidePotsInformation);
            }
            // update the players
            foreach (Player player in players)
            {
                CheckHasPlayer(player);
                playerToWrapper[player].Money = player.Money;
                playerToWrapper[player].PotInvestment = player.CurrentBet;
                updatePlayerCards(player, player.Cards);
            }

            if (onRoundOver)
                // when the round is over, reselect the winning players with their hand values.
                NotifyResultsIncomingCompleted();

            Respond(TimeSpan.FromMilliseconds(1500));
        }

        /// <summary>
        /// Assures the board order is displayed correctly according to the round order
        /// </summary>
        /// <remarks>
        /// When user connects to the game they don't appear in the right position with regards to the current player
        /// </remarks>
        private void assureBoardOrder()
        {
            // use the current player as a pivot
            int curPlayerIndex = roundOrder.FindIndex((cur) => cur.Name == board.ThePlayer.Name);
            // if the player isn't found, no need to repair
            if (curPlayerIndex > -1)
            {
                Player[] clone = new Player[roundOrder.Count - 1];
                int startIndex = curPlayerIndex + 1;
                int firstBlockLegnth = roundOrder.Count - startIndex;
                // copy the first block
                if (startIndex < roundOrder.Count)
                    roundOrder.CopyTo(startIndex, clone, 0, firstBlockLegnth);
                // copy the second block
                if (startIndex > -1)
                    roundOrder.CopyTo(0, clone, firstBlockLegnth, startIndex - 1);
                // repair the visual order
                board.OtherPlayers.Clear();
                foreach (Player player in clone)
                {
                    if (playerToWrapper.ContainsKey(player))
                    {
                        board.OtherPlayers.Add(playerToWrapper[player]);
                    }
                    else
                        doAddPlayer(player);

                }
            }
        }



        /// <summary>
        /// Assures the given player has a proper <see cref="GuiPlayerWrapper"/>. A new wrapper is created if there is none.
        /// </summary>
        /// <param name="player">The player to assure has a wrapper</param>
        /// <returns>
        /// True if the wrapper existed before the call. False if a new wrapper was created.
        /// </returns>
        protected bool CheckHasPlayer(Player player)
        {
            if (!playerToWrapper.ContainsKey(player))
            {
                doAddPlayer(player);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Provide the client card drawing capabilities
        /// </summary>
        /// <param name="player">The player which draws the cards</param>
        /// <param name="action">The action which must be modified to pass the player drawing actions</param>
        public void WaitPlayerDrawingAction(Player player, PlayerDrawingAction action)
        {
            // Set the new action
            this.drawingAction = action;
            // turn on only the draw action
            disableAllActions();
            updatePlayer(player, "Which cards would you like to draw?");
            updatePlayerCards(player, player.Cards);
            // disable hand highlighting so the player can draw the cards
            mainWindow.AutoHandHighlighting = false;
            board.ClearAllCardsSelection();

            board.ThePlayer.ChangeActionState(GuiActions.Draw, true);
            // register to the player drawing actions
            board.ThePlayer.PlayerDrawnCard += new EventHandler<CardDrawEventArgs>(ThePlayer_PlayerDrawnCard);
            board.ThePlayer.PlayerPerformedAction += new EventHandler<ActionEventArgs>(drawingActionPerformed);
        }

        /// <summary>
        /// A callback which is used to update the player drawing action.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void drawingActionPerformed(object sender, ActionEventArgs e)
        {
            if (e.Action == GuiActions.Draw)
            {
                resumeGamePlay();
            }
        }

        /// <summary>
        /// Assures the helper does not wait for any user input and restores the state
        /// </summary>
        private void resumeGamePlay()
        {
            // return the highlighting mechanism
            mainWindow.AutoHandHighlighting = true;
            // clear actions & events registration
            disableAllActions();
            drawingAction = null;
            betAction = null;
            board.ThePlayer.PlayerDrawnCard -= new EventHandler<CardDrawEventArgs>(ThePlayer_PlayerDrawnCard);
            board.ThePlayer.PlayerPerformedAction -= new EventHandler<ActionEventArgs>(drawingActionPerformed);
            board.ThePlayer.PlayerPerformedAction -= new EventHandler<ActionEventArgs>(ThePlayer_PlayerPerformedAction);
            board.ThePlayer.PlayerPerformedAction -= confirmFold;
            OnGuiClientResponded();
        }

        /// <summary>
        /// A callback for the player card drawing action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ThePlayer_PlayerDrawnCard(object sender, CardDrawEventArgs e)
        {
            // Get the card which was selected
            CardWrapper wrapper = e.Card;
            // flip the selection
            if (wrapper.IsSelected)
            {
                // card is de-selected, remove it from the list
                drawingAction.DrawnCards.Remove(wrapper.Card);
            }
            // assure the card drawing limit isn't reached and the card isn't already marked for drawing
            else if (drawingAction.DrawnCards.Count < 3 && !drawingAction.DrawnCards.Contains(wrapper.Card))
            {
                // mark the card to be drawn
                drawingAction.DrawnCards.Add(wrapper.Card);
            }
            // fix the card highlighting according to the cards selection
            foreach (CardWrapper temp in board.ThePlayer.Cards)
                temp.IsSelected = drawingAction.DrawnCards.Contains(temp.Card);

        }


        #endregion


        #region IGuiClientHelper Members

        /// <summary>
        /// This event is raised when the user aborts the game.
        /// </summary>
        public event DataEventHandler<bool> GameAborted;

        #endregion

        /// <summary>
        /// Gets the wrapper which was created for the given player
        /// </summary>
        /// <param name="player">The player of which to get the wrapper</param>
        /// <returns>The wrapper which is associated to the given player</returns>
        internal GuiPlayerWrapper this[Player player]
        {
            get { return playerToWrapper[player]; }
        }


        #region IClientHelper Members


        /// <summary>
        /// Called by the client to notify the current hand which is played
        /// </summary>
        /// <param name="currentHand">The current hand</param>
        public void NotifyCurrentHand(int currentHand)
        {
            board.BoardTitle = string.Format("Hand {0}", currentHand);
            OnGuiClientResponded();
        }

        #endregion

        /// <summary>
        /// Notify the Gui Helper of the already connected player
        /// </summary>
        /// <param name="players">A list of players which are currently connected to the server</param>
        public void NotifyLoggedinPlayers(IEnumerable<Player> players)
        {
            foreach (Player player in players)
            {
                updateAndLogPlayer(player, "Is Connected");
            }
            OnGuiClientResponded();
        }


        /// <summary>
        /// This method is called when there is a need for a time limit timer to start
        /// </summary>
        /// <param name="timeout">The remaining time of the player action
        /// </param>
        public void StartActionTimer(TimeSpan timeout)
        {
            // set the time limit value (multiply by the interval since in 
            // each interval the remaining time is reduced by one)
            int timeLimit = (int)timeout.TotalSeconds * TIMER_INTERVAL;
            mainWindow.MainPlayer.timeLimit.Maximum = timeLimit;
            board.ThePlayer.RemainingTime = timeLimit;
            timeLimiter.Start();
        }

        /// <summary>
        /// The callback to the player action time limit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timeLimiter_Tick(object sender, EventArgs e)
        {
            // reduce the remaining time by 1
            board.ThePlayer.RemainingTime = board.ThePlayer.RemainingTime - 1;
            // when the limit is reached, disable the input and wait for the server verdict
            if (board.ThePlayer.RemainingTime <= 0)
            {
                resumeGamePlay();
            }
        }



        #region IClientHelper Members


        /// <summary>
        /// Called by the client when a player disconnects
        /// </summary>
        /// <param name="player">The player which was disconnected</param>
        public void NotifyUserDisconnected(Player player)
        {
            updateAndLogPlayer(player, "Disconnected");
            Respond(TimeSpan.FromMilliseconds(250));
        }

        /// <summary>
        /// Called by the client to indicate that a player talked
        /// </summary>
        /// <param name="user">The talking user</param>
        /// <param name="message">The message which the user spoke</param>
        public void UserTalked(Player user, string message)
        {
            // ignore current user messages since they are displayed immidiatly
            if (user.Name != board.ThePlayer.Name)
            {
                doAddUserTalk(user, message);
            }
        }

        /// <summary>
        /// Adds the given message to the given player
        /// </summary>
        /// <param name="user">The speaking player</param>
        /// <param name="message">The player message</param>
        private void doAddUserTalk(Player user, string message)
        {
            Run run = new Run(string.Format("{0}: {1}{2}", user.Name, message, Environment.NewLine));
            run.Foreground = playerToWrapper[user].AssociatedColor;
            mainWindow.logContent.Inlines.Add(run);
            mainWindow.logBox.ScrollToEnd();
        }

        #endregion

        #region IGuiClientHelper Members

        private DataEventHandler<string> userChatDelegate;
        /// <summary>
        /// This event is raised when the user wants to send a chat message
        /// </summary>
        public event DataEventHandler<string> UserChats
        {
            // change the chat box visbility according to the delegate subscribers
            add
            {
                userChatDelegate += value;
                if (userChatDelegate != null)
                {
                    mainWindow.chatBox.Visibility = Visibility.Visible;
                }
            }
            remove
            {
                userChatDelegate -= value;
                if (userChatDelegate == null)
                {
                    mainWindow.chatBox.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Notify the Gui Helper that the server waits for this client to repond to close
        /// the server. Must raise <see cref="GameAborted"/> to signal the server to close.
        /// </summary>
        public void NotifyServerWaitsToClose()
        {
            string message = board.ThePlayer.Message;
            if (string.IsNullOrEmpty(message))
                message = "Press Logout to close the current session";
            else
                message = message + "\nPress Logout to close the current session";

            updatePlayer(board.ThePlayer.ThePlayer, message);
            disableAllActions();
            board.ThePlayer.ChangeActionState(GuiActions.Logout, true);
            board.ThePlayer.PlayerPerformedAction += new EventHandler<ActionEventArgs>(waitForLogoutToAbort);
            OnGuiClientResponded();
        }

        /// <summary>
        /// A callback to the player action to signal the player allows the server to close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void waitForLogoutToAbort(object sender, ActionEventArgs e)
        {
            disableAllActions();
            board.ThePlayer.PlayerPerformedAction -= new EventHandler<ActionEventArgs>(waitForLogoutToAbort);
            Running = false;
            OnGameAborted(false);
        }

        #endregion


        /// <summary>
        /// Gets an attached <see cref="ConcreteHelper"/>
        /// </summary>
        /// <param name="obj">The attached object</param>
        /// <returns>The attached <see cref="ConcreteHelper"/> or null, if none exists</returns>
        public static ConcreteHelper GetSingleHelper(DependencyObject obj)
        {
            return (ConcreteHelper)obj.GetValue(SingleHelperProperty);
        }

        /// <summary>
        /// Sets a single attached <see cref="ConcreteHelper"/>
        /// </summary>
        /// <param name="obj">The attached object</param>
        /// <param name="value">The new helper attached</param>
        public static void SetSingleHelper(DependencyObject obj, ConcreteHelper value)
        {
            obj.SetValue(SingleHelperProperty, value);
        }

        // Using a DependencyProperty as the backing store for SingleHelper.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The attached property which is used to mark a single helper on a given object
        /// </summary>
        public static readonly DependencyProperty SingleHelperProperty =
            DependencyProperty.RegisterAttached("SingleHelper", typeof(ConcreteHelper), typeof(ConcreteHelper), new UIPropertyMetadata(OnSingleHelperChanged));

        /// <summary>
        /// A callback for the SingleHelper property change
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnSingleHelperChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // assure the old helper detaches
            ConcreteHelper helper = e.OldValue as ConcreteHelper;
            if (helper != null)
                helper.detachFromWindow();
        }


        /// <summary>
        /// An <see cref="IEqualityComparer{Player}"/> implementation which compares players by their name
        /// </summary>
        private class NameComparer : IEqualityComparer<Player>
        {
            #region IEqualityComparer<Player> Members

            /// <summary>Determines whether the specified objects are equal.</summary>
            /// <returns>true if the specified objects are equal; otherwise, false.</returns>
            /// <param name="x">The first object of type <paramref name="T"/> to compare.</param>
            /// <param name="y">The second object of type <paramref name="T"/> to compare.</param>
            public bool Equals(Player x, Player y)
            {
                return x.Name.Equals(y.Name);
            }

            /// <summary>Returns a hash code for the specified object.</summary>
            /// <returns>A hash code for the specified object.</returns>
            /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
            /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
            public int GetHashCode(Player obj)
            {
                return obj.Name.GetHashCode();
            }

            #endregion
        }

    }
}
