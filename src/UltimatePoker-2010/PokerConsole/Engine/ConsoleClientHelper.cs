using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using PokerEngine;
using PokerRules.Hands;
using PokerRules.Deck;
using PokerEngine.Engine;
using PokerRules.Games;
using PokerService;

namespace PokerConsole.Engine
{
    /// <summary>
    /// A console base implementation of the <see cref="IClientHelper"/> interface.
    /// </summary>
    /// <remarks>
    /// This implementation uses the console to communicate with the player.
    /// </remarks>
    public class ConsoleClientHelper : IClientHelper
    {
        // a flag which indicates the player is sitting out
        private bool sittingOut = false;
        // the concrete client which this client helps. It is used to determine the player hands
        private IRulesInterpreter client;

        /// <summary>
        /// Creates a new instance of the ConsoleClientHelper
        /// </summary>
        /// <param name="client">The client which this helper helps.</param>
        public ConsoleClientHelper(IRulesInterpreter client)
        {
            this.client = client;
        }

        /// <summary>
        /// Called by the client when a connection is made successfuly to the server
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint which was opened locally.
        /// </param>
        public void NotifyConnectedToServer(EndPoint endPoint)
        {
            Console.WriteLine("Connected to " + endPoint);
        }

        /// <summary>
        /// Called by the client when a server identifies the running game.
        /// </summary>
        /// <param name="serverGame">The type of game the server is running</param>
        /// <param name="connectedPlayers">The number of players currently connected to the server</param>
        public void NotifyRunningGame(ServerGame serverGame, int connectedPlayers)
        {
            Console.WriteLine("Running game is {0}", serverGame);
            Console.WriteLine("Number of players signed in: {0}", connectedPlayers);
        }

        /// <summary>
        /// Gets the login name of the client. This method may be called again after a call to <see cref="NotifyNameExists"/></summary>
        /// <returns>A valid user name to login the client</returns>
        /// <remarks>
        /// This method must aquire the user login name. A new value must be returned after a call to <see cref="NotifyNameExists"/></remarks>
        public string GetName()
        {
            Console.WriteLine("What is your name?");
            return Console.ReadLine();
        }

        /// <summary>
        /// Called by the client when the name returned by <see cref="GetName"/> already exists on the server.
        /// </summary>
        /// <remarks>
        /// Derived classes may use this method to create a new login name.
        /// </remarks>
        public void NotifyNameExists()
        {
            Console.WriteLine("Name exists.");
        }

        /// <summary>
        /// Called by the client when a game is already in progress. The server can't be connected
        /// </summary>
        public void NotifyGameInProgress()
        {
            Console.WriteLine("Game in progress. Try again later");
        }

        /// <summary>
        /// Called by the client when a game is canceled by the server
        /// </summary>
        public void NotifyGameCanceled()
        {
            Console.WriteLine("Game canceled.");
        }

        /// <summary>
        /// Called by the client when a new player is connected.
        /// </summary>
        /// <param name="player">The new player which was connected</param>
        public void NotifyNewUserConnected(Player player)
        {
            Console.WriteLine("User {0} has joined the game", player);
        }

        /// <summary>
        /// Called by the client when the current client can't participate in the current round.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifyStartingGame"/> is called prior to the round start.
        /// </remarks>
        public void NotifySittingOut()
        {
            this.sittingOut = true;
            Console.WriteLine("You are sitting out this round.");
        }

        /// <summary>
        /// Called by the client when the current round starts and the client is participating.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifySittingOut"/> is called prior to the round start.
        /// </remarks>
        public void NotifyStartingGame()
        {
            Console.WriteLine("Staring game:");
        }

        /// <summary>
        /// Called by the client when a round starts.
        /// </summary>
        /// <param name="dealer">The current round dealer</param>
        /// <param name="potAmount">The starting amount of money in the pot</param>
        public void NotifyDealerAndPotAmount(Player dealer, int potAmount)
        {
            Console.WriteLine("Dealer is {0}", dealer.Name);
            Console.WriteLine("Pot amount is: {0}$", potAmount);
        }


        /// <summary>
        /// Called by the client when the blind open is made.
        /// </summary>
        /// <param name="opened">The player which blind opens</param>
        /// <param name="openAmount">The player open amount, may be 0</param>
        public void NotifyBlindOpen(Player opened, int openAmount)
        {
            Console.WriteLine("{0} blind opens with {1}$", opened.Name, openAmount);
        }

        /// <summary>
        /// Called by the client when the blind raise is made.
        /// </summary>
        /// <param name="raiser">The player which blind raises</param>
        /// <param name="raiseAmount">The raise amound, may be 0</param>
        public void NotifyBlindRaise(Player raiser, int raiseAmount)
        {
            if (raiseAmount > 0)
                Console.WriteLine("{0} blind raises with {1}$", raiser.Name, raiseAmount);
        }

        /// <summary>
        /// Called by the client when an update message arrives.
        /// </summary>
        /// <param name="player">The players sorted by their round order</param>
        /// <param name="potInformation">The current state of the pot</param>
        /// <param name="communityCards">The community cards in the game (if any) may be null or in 0 length</param>
        public virtual void WaitSynchronization(IEnumerable<Player> player, PotInformation potInformation, Card[] communityCards)
        {
            Console.WriteLine("Pot amount is: {0}$", potInformation.PotAmount);

            foreach (Player curPlayer in player)
            {
                PrintPlayer(curPlayer);
            }
        }


        /// <summary>
        /// The default implementation of the print player method.
        /// </summary>
        /// <param name="player"></param>
        protected virtual void PrintPlayer(Player player)
        {
            Console.Write(player);
            if (player.Cards.Count > 0)
            {
                // print all non-empty cards:
                foreach (Card card in player.Cards)
                {
                    if (card != Card.Empty)
                    {
                        Console.Write(", ");
                        Console.Write(card);
                    }
                }
                Hand hand = client.GetBestHand(player.Cards);
                // print the hand which was found, if any:
                if (hand != null)
                {
                    Console.WriteLine();
                    PrintHand(hand);
                }
            }
            Console.WriteLine();
        }
        /// <summary>
        /// Prints the given hand to the console.
        /// </summary>
        /// <param name="hand">The hand to print.</param>
        public void PrintHand(Hand hand)
        {
            Console.Write(hand);
            Console.Write(" (");
            foreach (Card card in hand)
            {
                Console.Write(card);
                Console.Write(' ');
            }
            Console.Write(')');
        }

        /// <summary>
        /// Called before a betting round starts.
        /// </summary>
        public void NotifyBetRoundStarted()
        {
            Console.WriteLine("Waiting for bets.");
        }

        /// <summary>
        /// Called when a betting round is completed and all players have checked.
        /// </summary>
        public void NotifyBetRoundComplete()
        {
            Console.WriteLine("No more bets");
        }

        /// <summary>
        /// Called when the betting round is completed and there is a single winner.
        /// </summary>
        public void NotifyAllFolded()
        {
            Console.WriteLine("There is a winner");
        }

        /// <summary>
        /// Called when a player is thinking of a move
        /// </summary>
        /// <param name="thinkingPlayer">The player which is thinking</param>
        public void NotifyPlayerIsThinking(Player thinkingPlayer)
        {
            Console.Write("{0} is thinking...  ", thinkingPlayer.Name);
        }

        /// <summary>
        /// Called by the client to get the current player betting action. Derived classes must implement it and respond with a 
        /// proper action
        /// </summary>
        /// <param name="player">The player which needs to bet</param>
        /// <param name="action">The action to use to pass the player response</param>
        public void WaitPlayerBettingAction(Player player, PlayerBettingAction action)
        {
            Console.WriteLine("====================");
            Console.WriteLine("{0}, would you like to:", player.Name);
            // the index which will be the fold action:
            int foldIndex = 3;

            if (!action.CanRaise)
                foldIndex = 2; // no "raise", fold will be in 2.

            if (action.CallAmount == 0)
            {
                // no amount of money to call, only check
                Console.WriteLine("1. Check.");
            }
            else if (action.IsAllInMode && action.CanRaise)
            {
                // the action is all in, can't check or call less than the player amount of money
                Console.WriteLine("1. All In {0}$", player.Money);
                // no "raise" fold will be in 2
                foldIndex = 2;
            }
            else
            {
                // there is an amount of money to call, print it:
                Console.WriteLine("1. Call {0}$", action.CallAmount);
            }
            if (!action.IsAllInMode && action.CanRaise)
            {
                // can raise, print the minimal amount
                Console.WriteLine("2. Raise {0}$", action.RaiseAmount);
            }
            // can always fold:
            Console.WriteLine("{0}. Fold", foldIndex);
            if (!action.IsAllInMode && action.CanRaise)
            {
                // when the player isn't forced to go "all in", it is the last option.
                Console.WriteLine("4. All In {0}$", player.Money);
            }
            Console.WriteLine("Enter a choice number or another amount to RAISE");

            int option;
            // loop until a valid int is parsed
            while (!int.TryParse(Console.ReadLine(), out option))
            {
                Console.WriteLine("[only numbers]");
            }

            // handle the user result:
            switch (option)
            {
                case 1: action.Call(); break;
                case 2: // fold or raise:
                    if (foldIndex == 2)
                        action.Fold();
                    else
                        action.Raise(action.RaiseAmount);
                    break;
                case 3: action.Fold(); break;
                case 4:// raise or call:
                    if (action.CanRaise)
                        action.Raise(player.Money - action.CallAmount);
                    else
                        action.Call();
                    break;
                default:
                    // verify other manual options
                    if (option < action.CallAmount || !action.CanRaise)
                    {
                        // can't raise, or the raise amount is lower than the call amount. change the option to call.
                        action.Call();
                    }
                    else if (option < action.RaiseAmount)
                        // validate an amount which is lower than the minimal raise.
                        option = action.RaiseAmount;

                    // in any case of a raise, make sure not to raise an amount which exceeds the player money.
                    if (option > player.Money - action.CallAmount)
                        option = player.Money - action.CallAmount;

                    if (action.Action != BetAction.CheckOrCall)
                        action.Raise(option);

                    break;
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
            Console.Write(player);
            Console.Write(' ');
            int totalAmount = callAmount + raiseAmount;
            if (betAction == BetAction.Raise && 0 == player.Money)
            {
                Console.WriteLine("is All In {0}$", totalAmount);
            }
            else
            {
                switch (betAction)
                {
                    case BetAction.CheckOrCall:

                        if (callAmount == 0)
                            Console.WriteLine("Checked");
                        else
                            Console.WriteLine("Called {0}$", callAmount);
                        break;
                    case BetAction.Raise:
                        if (callAmount == 0)
                            Console.WriteLine("Raised {0}$", raiseAmount);
                        else
                            Console.WriteLine("Called {0}$ & Raised {1}$", callAmount, raiseAmount);
                        break;
                    case BetAction.Fold: Console.WriteLine("Folds"); break;
                }
            }
        }

        /// <summary>
        /// Called when the betting round finalizes in any case the round is over.
        /// </summary>
        public void FinalizeBettingRound()
        {
            // wait 1250 milliseconds for the user before clearing the console
            System.Threading.Thread.Sleep(1250);
            Console.Clear();
        }

        /// <summary>
        /// Called by the client before the results starts to arrive.
        /// </summary>
        public void NotifyResultsIncomingStarted()
        {
            Console.WriteLine("Waiting for results");
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
                Console.WriteLine("Winner:");
                PrintPlayer(player);
            }
            else
                Console.WriteLine("{0} can't pay for the next round. LOSER!", player);
        }

        /// <summary>
        /// Called by the client after all of the results arrived
        /// </summary>
        public void NotifyResultsIncomingCompleted()
        {
            Console.WriteLine();
        }

        /// <summary>
        /// Called by the client when each round ends. 
        /// </summary>
        /// <param name="gameOver">A flag which indicates the game is over.</param>
        /// <param name="winner">The winning player (if any), may be null</param>
        public void WaitOnRoundEnd(bool gameOver, Player winner)
        {
            if (gameOver)
            {
                if (winner == null)
                    Console.WriteLine("Game was canceled.");
                else
                    Console.WriteLine("{0} wins!", winner);
            }
            else
                Console.WriteLine("Starting next round");

            Console.WriteLine(@"/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\");
            if (sittingOut)
                System.Threading.Thread.Sleep(500);
            else
            {
                Console.WriteLine("Press enter to continue");
                Console.ReadLine();
            }
            Console.Clear();
        }


        /// <summary>
        /// Called by the client to notify the current hand which is played
        /// </summary>
        /// <param name="currentHand">The current hand</param>
        public void NotifyCurrentHand(int currentHand)
        {
            Console.WriteLine("Current hand: {0}", currentHand);
        }

        /// <summary>
        /// Gets the interpretr which is used to interpret the game rules
        /// </summary>
        protected IRulesInterpreter Interpreter { get { return client; } }


        #region IClientHelper Members


        /// <summary>
        /// Called by the client when a player disconnects
        /// </summary>
        /// <param name="player">The player which was disconnected</param>
        public void NotifyUserDisconnected(Player player)
        {
            Console.WriteLine("User {0} disconnected", player);
        }

        /// <summary>
        /// Called by the client to indicate that a player talked
        /// </summary>
        /// <param name="user">The talking user</param>
        /// <param name="message">The message which the user spoke</param>
        public void UserTalked(Player user, string message)
        {
            // console helper does not support chatting
        }


        /// <summary>
        /// Called by the client when the connection to the server is closed
        /// </summary>
        public void NotifyConnectionClosed()
        {
            Console.WriteLine("Disconnected from server");
        }

        #endregion
    }
}
