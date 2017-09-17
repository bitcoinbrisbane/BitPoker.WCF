using System;
using System.Collections.Generic;
using System.Text;
using PokerEngine;
using BitPoker.Models.Hands;
using BitPoker.Models.Deck;
using PokerService;
using PokerRules.Games;

namespace PokerConsole.Engine
{
    /// <summary>
    /// A console based texas hold'em binary client implementation
    /// </summary>
    public class ConsoleTexasHoldemClient : ConsoleClientHelper
    {
        // the private card count, the default for texas hold'em is 2
        private int privateCardCount;

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="ConsoleTexasHoldemClient"/> class.</para>
        /// </summary>
        /// <param name="privateCardCount">The number of cards to print as player cards. The rest are printed as community cards
        /// </param>
        /// <param name="interpreter">The interpreter which provide game rules interpretation</param>
        public ConsoleTexasHoldemClient(IRulesInterpreter interpreter, int privateCardCount)
            : base(interpreter)
        {
            this.privateCardCount = privateCardCount;
        }

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="ConsoleTexasHoldemClient"/> class.</para>
        /// </summary>
        /// <param name="interpreter">The interpreter which provide game rules interpretation</param>
        public ConsoleTexasHoldemClient(IRulesInterpreter interpreter) : this(interpreter, 2) { } // 2 is the default "hole" cards of texas hold'em


        /// <summary>
        /// Prints the player, the player cards and hand if exists.
        /// </summary>
        /// <param name="player">The player to print</param>
        protected override void PrintPlayer(Player player)
        {
            Console.Write(player);
            // The player cards are exposed, print them:
            if (player.Cards.Count > 0 && !player.Cards.Contains(Card.Empty))
            {
                for (int i = 0; i < player.Cards.Count && i < privateCardCount; ++i)
                {
                    Console.Write(", ");
                    Console.Write(player.Cards[i]);
                }
                // print the community cards
                if (player.Cards.Count >= privateCardCount)
                {
                    Console.Write(" [");
                    for (int i = privateCardCount; i < player.Cards.Count; ++i)
                    {
                        if (i > privateCardCount)
                            Console.Write(", ");
                        Console.Write(player.Cards[i]);
                    }
                    Console.Write(']');
                }
                // print the player best hand
                Hand hand = Interpreter.GetBestHand(player.Cards);
                if (hand != null)
                {
                    Console.WriteLine();
                    PrintHand(hand);
                }
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Called by the client when an update message arrives.
        /// </summary>
        /// <param name="player">The players sorted by their round order</param>
        /// <param name="potInformation">The current state of the pot</param>
        /// <param name="communityCards">The community cards in the game (if any) may be null or in 0 length</param>
        public override void WaitSynchronization(IEnumerable<Player> player, PotInformation potInformation, Card[] communityCards)
        {
            // print the community cards first.
            if (communityCards.Length > 0)
            {
                Console.Write('[');
                foreach (Card card in communityCards)
                {
                    Console.Write(' ');
                    Console.Write(card);
                }
                Console.WriteLine(']');
            }
            base.WaitSynchronization(player, potInformation, communityCards);
        }
    }
}
