using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PokerRules.Games;
using PokerEngine.Betting;
using PokerRules.Hands;
using PokerRules.Deck;
using System.Collections.ObjectModel;
using PokerEngine.Debug;

namespace PokerEngine.Engine
{
    /// <summary>
    /// An abstract base class for poker engines. The engine is responsible of the game flow.
    /// </summary>
    /// <remarks>
    /// Various game types can derive this class and aquire basic game management capabilities.
    /// </remarks>
    public abstract class BaseEngine
    {
        /// <summary>
        /// Defines the maximal timeout limit which can be used with <see cref="ActionTimeout"/>
        /// </summary>
        public const int MAXIMAL_TIMEOUT = 90;
        // the current hand count
        private int currentHand = 0;
        // the first ante value is stored to auto increase
        private int firstAnte;
        // the first raise value is stored to auto increase
        private int firstRaise;

        // used to flag that the run has started.
        private bool hasRunStarted = false;

        // used to define tournament behaviors
        private bool tournamentMode = true;

        // a flag which indicates if the engine supports player additions after game start.
        private bool acceptPlayersAfterGameStart = false;

        // The current game played. It is used to deal the player cards, decide the winning players and hands.
        private BaseGame game;

        // The current round pot. It is used to manage the player bets and earnings.
        private SidePot pot;

        // The list of players which are still in the game
        private List<Player> players = new List<Player>();

        // The current round order. The round order is changed each round according to the dealer
        private List<Player> roundOrder = new List<Player>();

        // The current roudn playing players. It contains the players which didn't fold
        private List<Player> playingPlayers = new List<Player>();
        // The current dealer
        private int dealer = 0;
        // The player which starts after the blinds
        private int startingPlayer;

        /// <summary>
        /// Creates an instance of BaseEngine class
        /// </summary>
        protected BaseEngine()
        {
            // set default values
            RaiseLimit = 3;
            Ante = 100;
            SmallRaise = 250;
            StartingMoney = SmallRaise * 20;
            AutoIncreaseOnHandDivider = 7;
        }
        /// <summary>
        /// Gets or sets the Raise Limit. Default is 3.
        /// </summary>
        /// <remarks>
        /// The raise limit is the number of times the players can raise.
        /// </remarks>
        public int RaiseLimit { get; set; }

        /// <summary>
        /// Gets or sets the Ante value. Default is 100
        /// </summary>
        /// <remarks>
        /// The ante is the sum of money each player is obligated to bet in order to be part of the game round.
        /// </remarks>
        public int Ante { get; set; }

        /// <summary>
        /// Gets or sets the small raise. Default is 250
        /// </summary>
        /// <remarks>
        /// The small raise is the minimal amount of monety which can be raised.
        /// The blinds are calculated using this value, the blind open will be half of the value. The blind raise will raise to complete the
        /// first raise value to the Small Raise.
        /// </remarks>
        public int SmallRaise { get; set; }

        /// <summary>
        /// Gets or sets the starting money each player has. Default is 5000.
        /// </summary>
        public int StartingMoney { get; set; }

        /// <summary>
        /// Gets or sets the auto increase on hand divider. The default is 7.
        /// </summary>
        /// <remarks>
        /// In <see cref="TournamentMode"/>, the <see cref="Ante"/> and <see cref="SmallRaise"/> are raised automatically when the current
        /// hand is a divider of <c>AutoIncreaseOnHandDivider</c>
        /// </remarks>
        public int AutoIncreaseOnHandDivider { get; set; }

        private int timeout;
        /// <summary>
        /// Gets or sets the player action timeout.
        /// </summary>
        /// <remarks>
        /// Set a positive value to denote the number of seconds the player has to complete the action. Set a negative value to 
        /// indicate there is no time limit.
        /// </remarks>
        public int ActionTimeout
        {
            get { return timeout; }
            set
            {
                if (value < MAXIMAL_TIMEOUT)
                {
                    timeout = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the tournament mode. Default is true. You can't modify this method after the game has started.
        /// </summary>
        /// <remarks>
        /// In <c>TournamentMode</c>, the <see cref="Ante"/> and <see cref="SmallRaise"/> are raised automatically when the current
        /// hand is a divider of <see cref="AutoIncreaseOnHandDivider"/>. 
        /// Also, when a player is eliminated these value are increased.
        /// </remarks>
        public bool TournamentMode
        {
            get { return tournamentMode; }
            set
            {
                if (hasRunStarted)
                    throw new InvalidOperationException("Can't change the tournament mode after running");
                tournamentMode = value;
            }
        }

        /// <summary>
        /// Gets or sets a value which indicates if the engine accepts new players after game start.
        /// </summary>
        /// <remarks>
        /// This property will never return true when the <see cref="TournamentMode"/> flag is turned on.
        /// When this property is true, <see cref="GetNewPlayersAtRoundEnd"/> is called at each round end.
        /// </remarks>
        public bool AcceptPlayersAfterGameStart
        {
            get { return !tournamentMode && acceptPlayersAfterGameStart; }
            set { acceptPlayersAfterGameStart = value; }
        }

        /// <summary>
        /// Initializes the class. Call this after the constructor.
        /// </summary>
        public void Initialize()
        {
            OnInitialize();
        }

        /// <summary>
        /// Called when the class initializes by <see cref="Initialize"/>. Override it to add post constructor logic.
        /// </summary>
        protected virtual void OnInitialize()
        {

        }

        /// <summary>
        /// Gets the total amount of money in the game. 
        /// </summary>
        internal int TotalMoney { get; private set; }

        /// <summary>
        /// Gets the player betting data using the current <see cref="RoundOrder"/>
        /// </summary>
        /// <returns>
        /// A table as described by <see cref="SidePot.GetPlayersBettingData"/>
        /// </returns>
        public int[][] PlayersBettingData
        {
            get { return pot.GetPlayersBettingData(roundOrder); }
        }
        /// <summary>
        /// This event is fired when the game run starts, the delegate passes the signed in players.
        /// </summary>
        public event DataEventHandler<IEnumerable<Player>> GameStarted;

        /// <summary>
        /// Called by the engine before the first round with the signed in players
        /// </summary>
        /// <param name="signedInPlayers">The signed in players</param>
        protected virtual void OnGameStarted(IEnumerable<Player> signedInPlayers)
        {
            if (GameStarted != null)
                GameStarted(this, new DataEventArgs<IEnumerable<Player>>(signedInPlayers));
        }

        /// <summary>
        /// Runs the game until there is a single winner.
        /// </summary>
        /// <remarks>
        /// Call this method to start the game and play.
        /// </remarks>
        public void Run()
        {
            try
            {
                // mark the game as started
                hasRunStarted = true;
                // notify derived classes
                OnRunStarted();
                // get the players
                IEnumerable<Player> gotPlayers = WaitForPlayers().Distinct();
                // check maximal player limit
                if (gotPlayers.Count() > MaximalPlayersLimit)
                    gotPlayers = gotPlayers.Take(MaximalPlayersLimit);

                players.AddRange(gotPlayers);
                // set the players money and calculate the total amount of money
                players.ForEach((Player cur) => cur.Money = StartingMoney);
                TotalMoney = players.Count * StartingMoney;
                if (players.Count > 1)
                {
                    // notify game start
                    OnGameStarted(players);
                    // play until a winner exists
                    while (PlayNextRound())
                    {
                    }
                }
            }
#if DEBUG
            // in debug modes, use this break point to debug the failure
            catch
            {
                if (System.Diagnostics.Debugger.Launch())
                    System.Diagnostics.Debugger.Break();
                throw;
            }
#endif
            finally
            {
                // notify derived classes that the game run was completed
                OnRunComplete();
                // mark the game as over
                hasRunStarted = false;
            }
        }

        /// <summary>
        /// Called when the game run has started. Override it to add logic of game initialization
        /// </summary>
        protected virtual void OnRunStarted()
        {
            firstAnte = Ante;
            firstRaise = SmallRaise;
        }

        /// <summary>
        /// Called when the game run has completed. Override it to add cleanup logic
        /// </summary>
        protected virtual void OnRunComplete()
        {

        }

        /// <summary>
        /// Called by the engine to get the players for the game. Derived classes must override it and return a collection 
        /// of players.
        /// </summary>
        /// <returns>
        /// A non null enumerable of players (must have at least 2 players)
        /// </returns>
        protected abstract IEnumerable<Player> WaitForPlayers();

        /// <summary>
        /// Called by the engine in the begining of each round. Derived classes must override it and return a valid game instance.
        /// </summary>
        /// <returns>
        /// A non null game which will be used to manage the game logic.
        /// </returns>
        protected abstract BaseGame GetNewGame();

        /// <summary>
        /// Plays the next game round.
        /// </summary>
        /// <returns>
        /// True - if can play another round, False - to end the game
        /// </returns>
        private bool PlayNextRound()
        {
            // In debug mode, check that the total amount of money remaind.
            Invariant.CheckMoneySum(this);
            // Get a new game for the current round.
            game = GetNewGame();
            // Create a new side pot
            pot = new SidePot(players);
            // Increase hand count
            RaiseHandCount();
            // Initialize the game for the current amount of players.
            game.BeginGame(players.Count);
            // Map the playing players and the round order, blind open & blind raise
            mapPlayersAndBlindOpen();
            // finish the blind raise round
            handleFirstRaise();
            // Call derived class to continue the game round
            OnPlayNextRound();
            // find the winner(s) and distribute the earnings
            handleEndGame(game.EndGame());
            // since derived classes and helpers may remove players, verify there are any...
            if (players.Count == 0)
                return false;
            // In debug mode, check that the total amount of money remaind.
            Invariant.CheckMoneySum(this);
            // move the dealer chip one place
            ++dealer;
            dealer = dealer % players.Count;
            // Check if there are more players
            if (players.Count > 1)
            {
                // Use derive class to determine if another round should be played
                return WaitForAnotherRound();

            }
            else // there are no more players.
            {
                // Notify the game is over and stop the game
                WaitGameOver(players[0]);
                return false;
            }
        }

        /// <summary>
        /// Increases the hand count and notifies the current hand.
        /// </summary>
        private void RaiseHandCount()
        {
            ++currentHand;
            OnCurrentHandRaised(currentHand);
        }

        /// <summary>
        /// Called when the current hand is raised. Default implementation will use <see cref="AutoIncreaseOnHandDivider"/>
        /// to check automatic raise.
        /// </summary>
        /// <param name="currentHand">
        /// The current hand which is played
        /// </param>
        protected virtual void OnCurrentHandRaised(int currentHand)
        {
            // Test to see if need to increase the blinds.
            if (currentHand % AutoIncreaseOnHandDivider == 0)
                raiseBlinds();
        }

        /// <summary>
        /// Called when the round begins, manages the remainder of the betting round after the blind open &amp; raise
        /// </summary>
        private void handleFirstRaise()
        {
            // start the raising round from the startingPlayer indicator
            doRaise(startingPlayer % roundOrder.Count);
            // Notify derived class that the raise round has completed. Test to see if there are more players
            NotifyRaiseComplete(playingPlayers.Count == 1);
        }

        /// <summary>
        /// Called on each game round, override this method to add specific game logic.
        /// </summary>
        /// <remarks>
        /// This method is called after an early bet round.
        /// </remarks>
        protected abstract void OnPlayNextRound();

        /// <summary>
        /// Called after a betting round has completed
        /// </summary>
        /// <param name="allFolded">A flag indicating if all of the players folded and there is a single winner</param>
        protected virtual void NotifyRaiseComplete(bool allFolded)
        {

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
        protected abstract void WaitGameOver(Player winner);

        /// <summary>
        /// Called by the engine when the round is over. Override this method to determine if another round will be played.
        /// </summary>
        /// <returns>
        /// True - to indicate another round will be played, False - to finish the game.
        /// </returns>
        /// <remarks>
        /// On each round end, either this method or <see cref="WaitGameOver"/> will be called.
        /// </remarks>
        protected abstract bool WaitForAnotherRound();

        /// <summary>
        /// Maps the players to the game order and starts the betting round
        /// </summary>
        private void mapPlayersAndBlindOpen()
        {
            // clear previous round lists
            roundOrder.Clear();
            playingPlayers.Clear();
            for (int i = 0; i < players.Count; ++i)
            {
                // get real index starting from the dealer
                int realIndex = (dealer + i) % players.Count;
                Player curPlayer = players[realIndex];
                curPlayer.CurrentBet = 0;
                // enter the curPlayer to the player position in the current round order
                roundOrder.Add(curPlayer);
                // the first player will raise the Ante and the rest will call.
                if (i == 0)
                {
                    pot.Raise(curPlayer, Math.Min(Ante, curPlayer.Money));
                }
                else
                    pot.Call(curPlayer);
                // clear the old round cards and add the new round cards
                curPlayer.Cards.Clear();

                foreach (Card card in game.GetPlayerCards(i))
                {
                    curPlayer.Cards.Add(card);
                }
            }
            // mark all players as playing
            playingPlayers.AddRange(roundOrder);
            // reset the ante raise
            pot.ResetRaise();
            // notify derived classes of the pot money and dealer
            NotifyAntesAndDealer(pot.Money, roundOrder[0]);
            // start blind bets
            blindOpen();
        }

        /// <summary>
        /// Srtarts the blind bets
        /// </summary>
        private void blindOpen()
        {
            // get the blind opener & raiser, the dealer is the last to bet in the first round
            Player blindOpener = safeGetPlayer(1);

            Player blindRaiser = safeGetPlayer(2);
            // calculate initial amount to open & raise, in any case don't try to raise more than the player money
            int blindOpen = Math.Min(SmallRaise / 2, blindOpener.Money);
            // the blind raise needs to complete the bet to SmallRaise
            int blindRaise = Math.Min(SmallRaise, blindRaiser.Money) - blindOpen;

            // check if there are only 2 players remaining,
            if (playingPlayers.Count < 3)
            {
                // no blind raise if there are only 2 players.
                // recalculate the blind open to the full small raise.
                blindOpen = Math.Min(SmallRaise, blindOpener.Money);
                blindRaise = 0;
                startingPlayer = 2; // the starting player is the one who was the blind opener
            }

            // raise the blind open
            pot.Raise(blindOpener, blindOpen);

            // check if there are more than 2 players
            if (playingPlayers.Count > 2)
            {
                if (blindRaise <= 0) // check if blind raiser has enough money...
                {
                    blindRaise = 0;
                    pot.Call(blindRaiser);
                }
                else // raiser has enough money and needs to raise, so raise
                    pot.Raise(blindRaiser, blindRaise);
                // the starting player is the one after the blind raiser
                startingPlayer = 3;
            }
            // notify derived classes of blind open & blind raise
            NotifyBlindOpen(blindOpener, blindOpen);

            NotifyBlindRaise(blindRaiser, blindOpen, blindRaise);

        }

        /// <summary>
        /// This event is fired when the antes and delaer are set. The event arguments passes a pair which holds the pot amount
        /// and the dealer.
        /// </summary>
        public event DataEventHandler<KeyValuePair<int, Player>> AntesAndDealerSet;

        /// <summary>
        /// Called when the game round starts.
        /// </summary>
        /// <param name="potAmount">The initial pot amount</param>
        /// <param name="dealer">The current round dealer</param>
        protected virtual void NotifyAntesAndDealer(int potAmount, Player dealer)
        {
            if (AntesAndDealerSet != null)
                AntesAndDealerSet(this, new DataEventArgs<KeyValuePair<int, Player>>(new KeyValuePair<int, Player>(potAmount, dealer)));
        }

        /// <summary>
        /// This event is fired when a player performs a betting action.
        /// </summary>
        public event EventHandler<PlayerActionEventArgs> PlayerPerformedAction;

        private void raisePlayerPerformedBet(Player player, BetAction betAction, int callAmount, int raiseAmount, bool isBlindAction)
        {
            if (PlayerPerformedAction != null)
                PlayerPerformedAction(this, new PlayerActionEventArgs(player, betAction, callAmount, raiseAmount, isBlindAction));
        }

        /// <summary>
        /// Called when the game round starts and the blind open is made
        /// </summary>
        /// <param name="opener">The blind opener</param>
        /// <param name="openAmount">The open amount, can be 0</param>
        protected virtual void NotifyBlindOpen(Player opener, int openAmount)
        {
            raisePlayerPerformedBet(opener, BetAction.Raise, 0, openAmount, true);
        }

        /// <summary>
        /// Called when the game round starts and the blind raise is made
        /// </summary>
        /// <param name="raiser">The blind raiser</param>
        /// <param name="raiseAmount">The raise amount, can be 0</param>
        /// <param name="openAmount">The original open amount which was notified by <see cref="NotifyBlindOpen"/></param>
        protected virtual void NotifyBlindRaise(Player raiser, int openAmount, int raiseAmount)
        {
            if (startingPlayer > 2)
            {
                BetAction action = BetAction.Raise;
                if (raiseAmount == 0)
                    action = BetAction.CheckOrCall;
                raisePlayerPerformedBet(raiser, action, openAmount, raiseAmount, true);
            }
        }

        /// <summary>
        /// Called by the engine in various occasions to update the players information. 
        /// </summary>
        /// <param name="orderedPlayers">The players in their current round order</param>
        /// <param name="potAmount">The current pot amount</param>
        protected abstract void WaitSynchronizePlayers(IEnumerable<Player> orderedPlayers, int potAmount);

        /// <summary>
        /// Synchronizes the player cards and calls <see cref="WaitSynchronizePlayers(IEnumerable{Player},int)"/>
        /// </summary>
        protected void WaitSynchronizePlayers()
        {
            updatePlayerCards();
            WaitSynchronizePlayers(playingPlayers, pot.Money);
        }

        /// <summary>
        /// updates the player cards using current round order and the current game
        /// </summary>
        private void updatePlayerCards()
        {
            // go over each player using their defined round order
            for (int i = 0; i < roundOrder.Count; ++i)
            {
                // clear the old cards
                ObservableCollection<Card> playerCards = roundOrder[i].Cards;
                playerCards.Clear();
                // add the new cards
                foreach (Card card in game.GetPlayerCards(i))
                {
                    playerCards.Add(card);
                }
            }
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
        protected abstract void WaitPlayerBettingAction(Player player, PlayerBettingAction action);

        /// <summary>
        /// Starts a raise round using the given player as the first player to bet.
        /// </summary>
        /// <param name="startingPlayer">The player to start with. It can't be null and must be an active player
        /// </param>
        protected void DoRaiseRound(Player startingPlayer)
        {
            doRaise(roundOrder.IndexOf(startingPlayer));
        }

        /// <summary>
        /// Starts a raise round with the player <b>after the dealer</b> as the first player to bet
        /// </summary>
        protected void DoRaiseRound()
        {
            doRaise(1);
        }

        /// <summary>
        /// Gets the current round Dealer
        /// </summary>
        protected Player Dealer
        {
            // the dealer is defined as the first player
            get { return safeGetPlayer(0); }
        }

        /// <summary>
        /// Determines if there are anymore playing players in the current round.
        /// </summary>
        public bool HasRoundPlayers
        {
            // when the players count is 1 that means there is a single winner
            get { return playingPlayers.Count > 1; }
        }

        /// <summary>
        /// Determines if the given player has folded or the player is part of the current round
        /// </summary>
        /// <param name="player">Any player value, can't be null.</param>
        /// <returns>
        /// True if the player didn't fold in the current round.
        /// </returns>
        protected bool HasPlayerFolded(Player player)
        {
            return playingPlayers.Contains(player);
        }

        /// <summary>
        /// Gets the player index in the current round according to the mapping
        /// </summary>
        /// <param name="player">The player to look for.</param>
        /// <returns>The index of the player or -1 if the player isn't in the round order</returns>
        protected int GetPlayerGameIndex(Player player)
        {
            return roundOrder.IndexOf(player);
        }

        /// <summary>
        /// Gets the players which are part of the game
        /// </summary>
        internal IEnumerable<Player> Players { get { return players; } }
        /// <summary>
        /// Gets the players which are playing in the current round
        /// </summary>
        protected internal IEnumerable<Player> PlayingPlayers { get { return playingPlayers; } }
        /// <summary>
        /// Gets the playing players ordered by their round order
        /// </summary>
        protected IEnumerable<Player> RoundOrder { get { return roundOrder; } }

        /// <summary>
        /// Determines if the round players are all in. 
        /// </summary>
        protected bool RoundPlayersAreAllIn
        {
            get
            {
                // round players are all in when there is at most one player with money amount larger than 0 and 
                // all players have no debts to the pot
                return playingPlayers.Count((cur) => cur.Money > 0) < 2 &&
                    playingPlayers.TrueForAll((cur) => pot.GetPlayerCallSum(cur) == 0 || cur.Money == 0);
            }
        }

        /// <summary>
        /// Runs a betting round starting from the given player
        /// </summary>
        /// <param name="startingPlayer">The index of the player in the round order to start from</param>
        private void doRaise(int startingPlayer)
        {
            // update player cards
            WaitSynchronizePlayers();
            // check if there are any players which can bet
            if (!RoundPlayersAreAllIn)
            {
                // the current player
                int curPlayer = startingPlayer;
                // a counter to count the number of players who checked. it reverts to 0 after each raise
                int allCheck = 0;
                // a counter to count the number of raise actions, it simply increases on each player
                int raiseCount = 0;
                // the raise limit for this round, it is calculated using the initial playingPlayers count * the raise limit.
                // this way each player will have (RaiseLimit) opportunities to raise.
                int curRaiseLimit = playingPlayers.Count * RaiseLimit;
                // a flag which marks an "all in" round in which all of the players are obligated to either go "all in" or fold
                bool isAllInMode = false;
                // loop as long as there are round players & there are players which need to call/check
                while (allCheck != playingPlayers.Count && HasRoundPlayers)
                {
                    // Get the current player according to the round order
                    Player player = safeGetPlayer(curPlayer);
                    // skip the player if the player has folded
                    if (playingPlayers.Contains(player))
                    {
                        // check that this player has any money to gamble with or the player is "all in"
                        if (player.Money > 0)
                        {
                            // create a new betting action for this player
                            PlayerBettingAction action = GetPlayerBettingAction(player, isAllInMode, raiseCount < curRaiseLimit);
                            // call derived class to provide the player bet
                            WaitPlayerBettingAction(player, action);
                            // check raise restrictions in case of a raise:
                            if (action.Action == BetAction.Raise)
                            {
                                // assure the raise is higher than the SmallRaise
                                if (action.RaiseAmount < SmallRaise)
                                    action.Raise(SmallRaise);

                                // assure the current raise amount does not exceed the player money
                                if (action.RaiseAmount + action.CallAmount > player.Money)
                                {
                                    action.Raise(player.Money - action.CallAmount);
                                }
                            }
                            // action handling:
                            switch (action.Action)
                            {
                                case BetAction.CheckOrCall:
                                    pot.Call(player);
                                    if (isAllInMode && player.Money > 0) // in "all in" mode, the player must play all of the player money
                                        pot.Raise(player, player.Money);
                                    ++allCheck; // increase the check count to progres, so when all players check the round will continue
                                    break;
                                case BetAction.Raise:
                                    // raise the player raise amount
                                    pot.Raise(player, action.RaiseAmount);
                                    // reset the check count so other players will have to call/check
                                    allCheck = 1;
                                    break;
                                case BetAction.Fold:
                                    // remove the player from the pot, game & playingPlayers
                                    pot.Fold(player);
                                    game.FoldPlayer(curPlayer);

                                    playingPlayers.Remove(player);
                                    break;
                            }
                            /*
                            if (action.Action != BetAction.Fold && tournamentMode)
                            {
                                isAllInMode = action.IsAllInMode || player.Money == 0;
                            }
                             */
                            // Notify derived classes on player action
                            NotifyPlayerAction(player, action.Action, action.CallAmount, action.RaiseAmount);
                        }
                        else // player has no money so the player can't raise,
                            // the player is "all in" so the player doesn't need to fold.
                            ++allCheck;
                        // In debug mode, verify that the playing players contributed the same amount of money to the pot.
                        Invariant.VerifyPotIsEven(this, player, pot);

                    }
                    // move the current player to the next in the current round order.
                    curPlayer = (curPlayer + 1) % roundOrder.Count;
                    // increase the raise counter (which is never zeroed) to restrict the raise limit
                    ++raiseCount;
                }
                Invariant.VerifyPlaingPlayersBet(playingPlayers, pot);
                Invariant.VerifyPotIsEven(this, pot);
                // reset the current round raise
                pot.ResetRaise();
            }
        }


        /// <summary>
        /// Called after each player performs an action
        /// </summary>
        /// <param name="player">The player which performed the action </param>
        /// <param name="betAction">The action performed</param>
        /// <param name="callAmount">The amount which the player had to call</param>
        /// <param name="raiseAmount">The amount which the player raised (if any)</param>
        protected virtual void NotifyPlayerAction(Player player, BetAction betAction, int callAmount, int raiseAmount)
        {
            raisePlayerPerformedBet(player, betAction, callAmount, raiseAmount, false);
        }

        /// <summary>
        /// Gets a new player betting action for the current player.
        /// </summary>
        /// <param name="player">The player for which the action is created</param>
        /// <param name="seenAllIn">A flag indicating if the current betting round is an "all in" round</param>
        /// <param name="canRaise">A flag indicating if the current betting round allows any raise</param>
        /// <returns>
        /// A new player betting action which then must be performed by the player
        /// </returns>
        /// <remarks>
        /// This method considers the player money and the round restrictions to generate a new <see cref="PlayerBettingAction"/>
        /// </remarks>
        private PlayerBettingAction GetPlayerBettingAction(Player player, bool seenAllIn, bool canRaise)
        {
            // The initial raise amount is set to the SmallRaise
            int raiseAmount = SmallRaise;
            // Get the player debt to the pot
            int playerCallSum = pot.GetPlayerCallSum(player);
            // The isAllInMode flag, it idicates if the player must go "all in" or fold
            bool isAllInMode = false;
            // if the player call sum is higher than the player money, he may only go "all in"
            if (playerCallSum >= player.Money || seenAllIn)
            {
                isAllInMode = true;
                // fix the raise amount to the difference between the player call sum and the player money
                if (playerCallSum + raiseAmount > player.Money)
                    raiseAmount = player.Money - playerCallSum;
                // the previous calculation can result in a negative amount, make sure no to pass the player a negative raise amount
                if (raiseAmount < 0)
                    raiseAmount = 0;
                // in an "all in" mode the player must call all of the available money
                playerCallSum = player.Money;
            }
            return new PlayerBettingAction(playerCallSum, raiseAmount, isAllInMode, canRaise);
        }

        /// <summary>
        /// Called when a player wins a round. There can be more than one winner per round.
        /// </summary>
        /// <param name="player">The winning player</param>
        /// <param name="result">The winning player hand</param>
        protected virtual void OnDeclareWinner(Player player, GameResult result)
        {
        }

        /// <summary>
        /// Called when a player loses the game. The player has lost all of the money and can't continue in the game.
        /// </summary>
        /// <param name="player">The losing player</param>
        /// <remarks>
        /// In a <see cref="TournamentMode"/> the <see cref="Ante"/> and the <see cref="SmallRaise"/> are increased.
        /// </remarks>
        protected virtual void OnDeclareLoser(Player player)
        {
            raiseBlinds();
        }

        /// <summary>
        /// Raises the <see cref="Ante"/> and the <see cref="SmallRaise"/>.
        /// </summary>
        /// <remarks>
        /// In <see cref="TournamentMode"/> the players are forced to reach a decision by increasing the minimal amounts of money 
        /// which are part of each hand played.
        /// </remarks>
        private void raiseBlinds()
        {
            if (tournamentMode)
            {
                // increase the ante & small raise by their initial amounts.
                Ante += firstAnte;
                SmallRaise += firstRaise;
                // assure the blind openings won't increase higher than the total amount of money played in the game divided by 2
                if (SmallRaise > TotalMoney / 2)
                    SmallRaise = TotalMoney / 2;
            }
        }

        /// <summary>
        /// Finishes the current round of the game.
        /// </summary>
        /// <param name="gameResult">The results passed by the game</param>
        /// <remarks>
        /// This method will notify the winner(s) and distribute the earnings.
        /// </remarks>
        private void handleEndGame(GameResult[] gameResult)
        {
            WaitSynchronizePlayers();
            // The collection of winners to pass to the pot for the split
            List<Player> winners = new List<Player>();
            foreach (GameResult result in gameResult)
            {
                // Get the winner out of the current round order
                Player winner = roundOrder[result.Player];
                // Notify derived classes of the winner
                OnDeclareWinner(winner, result);
                // add to the list of winners to share the earnings
                winners.Add(winner);
            }
            // distribute the pot
            pot.SplitPot(comparePlayers);
            // A queue of losers which is passed to the derived classes and can be extended to remove players.
            Queue<Player> toRemove = new Queue<Player>();
            foreach (Player player in players)
            {
                // Automatically add as a loser any player with no money.
                if (player.Money == 0)
                {
                    // notify derived classes that the player will be kicked.
                    OnDeclareLoser(player);
                    toRemove.Enqueue(player);
                }
            }
            // Pass the queue of losers to the derived classes, the queue might be modified.
            NotifyWinLoseComplete(toRemove);
            // finally, drop the players which were marked as losers.
            while (toRemove.Count > 0)
            {
                Player current = toRemove.Dequeue();
                // remove the player from the players roster, and if the player had any money left, decrease it from the total sum/
                if (players.Remove(current) && current.Money > 0)
                    TotalMoney -= current.Money;

            }
            // check if accepts new players
            if (AcceptPlayersAfterGameStart)
            {
                // recycle the empty queue to the new queue
                Queue<Player> toAdd = toRemove;
                GetNewPlayersAtRoundEnd(toAdd);
                // calculate the number of open positions in the game
                int playerLimit = game.MaximalPlayersLimit - players.Count;
                int counter = 0;
                // loop as long as there is room for new players and there are players waiting to join
                while (toAdd.Count > 0 && playerLimit > counter)
                {
                    Player newPlayer = toAdd.Dequeue();
                    // the call to GetNewPlayersAtRoundEnd() should have added the money to the players
                    if (newPlayer.Money > 0)
                    {
                        TotalMoney += newPlayer.Money;
                        players.Add(newPlayer);
                        // count only added players
                        ++counter;
                    }
                }
            }
            // update the derived class with the new players roster
            WaitSynchronizePlayers();
        }

        private int comparePlayers(Player x, Player y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;
            Hand leftHand = null;
            Hand rightHand = null;
            // get hands for participating players
            if (playingPlayers.Contains(x))
                leftHand = game.GetBestHand(x.Cards);
            if (playingPlayers.Contains(y))
                rightHand = game.GetBestHand(y.Cards);
            // if both player don't have a hand their equal
            if (leftHand == null && rightHand == null)
                return 0;
            // if one doesn't have a hand but the other does, the other is better
            if (leftHand == null ^ rightHand == null)
            {
                if (leftHand == null)
                    return -1;
                return 1;
            }
            // have both hands, comprae using it:
            return leftHand.CompareTo(rightHand);
        }

        /// <summary>
        /// Called at the round end to allow dervied classes to add new players. 
        /// </summary>
        /// <param name="toAdd">The queue of players to add to new players.</param>
        /// <remarks>
        /// This method called only when the game isn't in a <see cref="TournamentMode"/>
        /// <note>Note that the newly added players must have their initial money initialized to a positive sum.</note>
        /// </remarks>
        protected virtual void GetNewPlayersAtRoundEnd(Queue<Player> toAdd)
        {

        }

        /// <summary>
        /// This event is fired when the round is over.
        /// </summary>
        public event EventHandler RoundOver;

        /// <summary>
        /// Called after each round with a queue of losing players, the queue can be modified.
        /// </summary>
        /// <param name="losers">The queue of losers.</param>
        /// <remarks>
        /// Derived classes may modify the queue to add/remove losers.
        /// </remarks>
        protected virtual void NotifyWinLoseComplete(Queue<Player> losers)
        {
            if (RoundOver != null)
            {
                RoundOver(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the player with the given index from the round order, the index will be normalized to the proper position.
        /// </summary>
        /// <param name="index">The index of the player to get</param>
        /// <returns>The player in the round order in the given index</returns>
        private Player safeGetPlayer(int index)
        {
            // assure the index is in range
            index = index % roundOrder.Count;
            return roundOrder[index];
        }

        /// <summary>
        /// Gets the player best hand or null if none exists.
        /// </summary>
        /// <param name="player">The player of which to get the hand</param>
        /// <returns>The player best hand according to the current game logic, or null if no hand exists or the player is null.</returns>
        public Hand GetBestHand(Player player)
        {
            int index = roundOrder.IndexOf(player);
            if (index == -1) // player was not found
                return null;
            // get the player hand using the game logic
            return game.GetPlayerBestHand(index);
        }

        /// <summary>
        /// Gets the maximal player count
        /// </summary>
        public int MaximalPlayersLimit
        {
            get
            {
                if (game == null)
                    game = GetNewGame();
                return game.MaximalPlayersLimit;
            }
        }

    }
}
