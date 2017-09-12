using System;
using System.Collections.Generic;
using System.Text;
using PokerEngine;
using PokerEngine.Engine;
using PokerRules.Deck;
using PokerRules.Games;

namespace PokerConsole.Engine
{
    /// <summary>
    /// A console based implementation of the five card draw binary client
    /// </summary>
    public class ConsoleFiveGameDrawClient : ConsoleClientHelper, IFiveCardClientHelper
    {

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="ConsoleFiveGameDrawClient"/> class.</para>
        /// </summary>
        public ConsoleFiveGameDrawClient(IRulesInterpreter client)
            : base(client) // pass null to the base class but fix it later
        {

        }

        /// <summary>
        /// Called by the client when a drawing round starts
        /// </summary>
        public void NotifyDrawingRoundStarted()
        {
            Console.WriteLine("Waiting for draws.");
        }

        /// <summary>
        /// Called by the client when a drawing round is finalized.
        /// </summary>
        public void FinalizeDrawingRound()
        {
            // waits a second before celearing the console so the user can read.
            System.Threading.Thread.Sleep(1000);
            Console.Clear();
        }

        /// <summary>
        /// Called by the client to notify how many cards were drawn by the player
        /// </summary>
        /// <param name="player">The drawing players</param>
        /// <param name="drawCount">The amount of cards drawn. Can be 0</param>
        public void NotifyPlayerDraws(Player player, int drawCount)
        {
            if (drawCount == 0)
            {
                Console.WriteLine("Player {0} stand pats", player);
            }
            else
                Console.WriteLine("Player {0} draws {1} cards", player, drawCount);
        }

        /// <summary>
        /// Updates the current client with the new cards drawn.
        /// </summary>
        /// <param name="player">
        /// The player with the new updated cards
        /// </param>
        public void NotifyPlayerNewCards(Player player)
        {
            PrintPlayer(player);
        }

        /// <summary>
        /// Called by the client to get the player drawing response.
        /// </summary>
        /// <param name="player">The player which draws the cards</param>
        /// <param name="action">The action which should be modified according to the player actions</param>
        public void WaitPlayerDrawingAction(Player player, PlayerDrawingAction action)
        {
            PrintPlayer(player);
            Console.WriteLine("=======================");
            Console.WriteLine("{0}, which cards would you like to draw?", player.Name);
            // prompt the user for the cards to draw:
            string[] numbers = Console.ReadLine().Split(',');
            // only 3 cards are allowed to draw
            int max = Math.Min(numbers.Length, 3);
            List<Card> drawnCards = action.DrawnCards;
            for (int j = 0; j < max; ++j)
            {
                int curCard;
                // skip unknown card format
                if (int.TryParse(numbers[j], out curCard))
                {
                    --curCard;
                    drawnCards.Add(player.Cards[curCard]);
                }
            }
        }

        #region IFiveCardClientHelper Members


        /// <summary>
        /// Called by the client when a drawing round completes
        /// </summary>
        public void NotifyDrawingRoundCompleted()
        {

        }

        #endregion
    }
}
