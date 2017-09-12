using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerConsole.Engine;
using PokerRules.Deck;
using PokerRules.Hands;
using PokerEngine;
using PokerRules.Games;

namespace UltimatePoker.Engine
{
    /// <summary>
    /// A simple TCP/IP based, WPF based client of the Omaha Hold'em game.
    /// </summary>
    public class OmahaHoldemGuiClient : TexasHoldemGuiClient
    {
        // The game is updated with the community cards received by the server
        private OmahaHoldem game;

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="OmahaHoldemGuiClient"/> class.</para>
        /// </summary>
        /// <param name="userName">The initial user name which the client will try to register
        /// </param>
        public OmahaHoldemGuiClient(string userName)
            : base(userName, 4)// omaha hold'em has 4 private "hole" cards
        {

        }

        /// <summary>
        /// Creates a new instance of a game. Derived classes must return a valid instance.
        /// </summary>
        /// <returns>A new instance of the game class</returns>
        /// <remarks>
        /// The game is used to provide client side capabilities regarding the poker game rules.
        /// </remarks>
        protected override BaseGame GetNewGame()
        {
            game = new OmahaHoldem();
            return game;
        }

        /// <summary>
        /// Called by the client when an update message arrives.
        /// </summary>
        /// <param name="player">The players sorted by their round order</param>
        /// <param name="potAmount">The current amount of money in the pot</param>
        /// <param name="potData">The current pot investement data ordered by the players starting with the dealer</param>
        /// <param name="communityCards">The community cards in the game (if any) may be null or in 0 length</param>
        protected override void WaitSynchronization(IEnumerable<Player> player, int potAmount, int[,] potData, Card[] communityCards)
        {
            // manually update the community cards so the client will know how to calculate player hands
            game.ExposedCommunityCards = Array.AsReadOnly<Card>(communityCards);

            base.WaitSynchronization(player, potAmount, potData, communityCards);
        }
    }
}
