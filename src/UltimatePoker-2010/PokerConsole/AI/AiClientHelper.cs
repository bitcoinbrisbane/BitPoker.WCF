using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerConsole.Engine;
using PokerEngine;
using PokerEngine.Engine;
using PokerRules.Games;
using PokerService;
using BitPoker.Models.Deck;

namespace PokerConsole.AI
{
    /// <summary>
    /// A basic client helper which provides automated game plays.
    /// </summary>
    public class AiClientHelper : ClientHelperDecorator
    {
        // the current round strategy. it is chosen when each round starts.
        private AIStrategy currentStrategy = new EmptyStrategy();
        // the helper random number generator
        private Random rand = new Random();
        // the current ai login name
        private string aiName = "AI.1";
        // a counter which increases every rejected ai name. It is used to compose a new AI name...
        private int rejectionCount = 0;
        // the client used to provide game rules
        private IRulesInterpreter client;

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="AiClientHelper"/> class.</para>
        /// </summary>
        /// <param name="client">The client which this helper helps.
        /// </param>
        /// <param name="helper">The concrete helper which is used for unimportant I/O</param>
        public AiClientHelper(IClientHelper helper, IRulesInterpreter client)
            : base(helper)
        {
            this.client = client;
        }

        #region IClientHelper Members


        /// <summary>
        /// Gets the login name of the client. This method may be called again after a call to <see cref="NotifyNameExists"/></summary>
        /// <returns>A valid user name to login the client</returns>
        /// <remarks>
        /// This method must aquire the user login name. A new value must be returned after a call to <see cref="NotifyNameExists"/></remarks>
        public override string GetName()
        {
            return aiName;
        }


        /// <summary>
        /// Called by the client when a round starts.
        /// </summary>
        /// <param name="dealer">The current round dealer</param>
        /// <param name="potAmount">The starting amount of money in the pot</param>
        public override void NotifyDealerAndPotAmount(PokerEngine.Player dealer, int potAmount)
        {
            chooseStrategy();
            base.NotifyDealerAndPotAmount(dealer, potAmount);
        }

        /// <summary>
        /// Chooses a random strategy to play with the next round.
        /// </summary>
        private void chooseStrategy()
        {
            // select the next strategy at random
            int decision = rand.Next(4);
            switch (decision)
            {
                case 0: currentStrategy = new SimpleLimitStrategy(rand.Next(250, 5000)); break;
                case 1: currentStrategy = new RandomCallRaise(70); break;
                case 2: currentStrategy = new BetterLimitStrategy(rand.Next(250, 5000), client); break;
                case 3: currentStrategy = new SimpleFiveCardStrategy(client); break;
            }

        }


        /// <summary>
        /// Called by the client when the name returned by <see cref="GetName"/> already exists on the server.
        /// </summary>
        /// <remarks>
        /// Derived classes may use this method to create a new login name.
        /// </remarks>
        public override void NotifyNameExists()
        {
            ++rejectionCount;
            aiName = string.Format("AI.{0}", rejectionCount);
        }


        /// <summary>
        /// Called by the client when each round ends. 
        /// </summary>
        /// <param name="gameOver">A flag which indicates the game is over.</param>
        /// <param name="winner">The winning player (if any), may be null</param>
        public override void WaitOnRoundEnd(bool gameOver, PokerEngine.Player winner)
        {

        }

        /// <summary>
        /// Called by the client to get the current player betting action. Derived classes must implement it and respond with a 
        /// proper action
        /// </summary>
        /// <param name="player">The player which needs to bet</param>
        /// <param name="action">The action to use to pass the player response</param>
        public override void WaitPlayerBettingAction(PokerEngine.Player player, PokerEngine.Engine.PlayerBettingAction action)
        {
            currentStrategy.Bet(player, action);
        }

        /// <summary>
        /// Called by the client when an update message arrives.
        /// </summary>
        /// <param name="player">The players sorted by their round order</param>
        /// <param name="potInformation">The current state of the pot</param>
        /// <param name="communityCards">The community cards in the game (if any) may be null or in 0 length</param>
        public override void WaitSynchronization(IEnumerable<Player> player, PotInformation potInformation, Card[] communityCards)
        {
            currentStrategy.Synchronize(player);
            base.WaitSynchronization(player, potInformation, communityCards);
        }

        #endregion

        /// <summary>
        /// Called by the client to get the player drawing response.
        /// </summary>
        /// <param name="player">The player which draws the cards</param>
        /// <param name="action">The action which should be modified according to the player actions</param>
        public void WaitPlayerDrawingAction(Player player, PlayerDrawingAction action)
        {
            currentStrategy.Draw(player, action);
        }
    }
}
