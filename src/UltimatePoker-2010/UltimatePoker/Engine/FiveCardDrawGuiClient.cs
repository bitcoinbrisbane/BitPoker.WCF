using System;
using System.Collections.Generic;
using System.Text;
using PokerConsole.Engine;
using PokerEngine;
using PokerEngine.Engine;
using BitPoker.Models.Deck;
using System.Linq;
using BitPoker.Models.Hands;
using PokerRules.Games;

namespace UltimatePoker.Engine
{
    /// <summary>
    /// A simple gui client of the five card draw game.
    /// </summary>
    public class FiveCardDrawGuiClient : ConcreteHelper, IFiveCardGuiClientHelper
    {

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="FiveCardDrawGuiClient"/> class.</para>
        /// </summary>
        /// <param name="initialName">The initial user name which is the login name of the player
        /// </param>
        /// <param name="client">The client which runs the game, it is used to get the player hands
        /// </param>
        public FiveCardDrawGuiClient(string initialName, IRulesInterpreter client)
            : base(initialName, client)
        {
        }
        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="FiveCardDrawGuiClient"/> class.</para>
        /// </summary>
        /// <param name="initialName">The initial user name which is the login name of the player
        /// </param>
        /// <param name="client">The client which runs the game, it is used to get the player hands
        /// </param>
        public FiveCardDrawGuiClient(IRulesInterpreter client)
            : base(client)
        {
        }

        /// <summary>
        /// Updates the player cards With the given cards
        /// </summary>
        /// <param name="wrapper">The player to update</param>
        /// <param name="cards">The player new cards</param>
        protected override void UpdatePlayerCards(GuiPlayerWrapper wrapper, IEnumerable<Card> cards)
        {
            // clear old cards
            wrapper.Cards.Clear();
            if (cards != null)
            {
                // try getting the player hand
                Hand hand = Client.GetBestHand(cards);
                if (hand != null)
                    // found a hand, upadte the player
                    wrapper.CurrentHand = hand;

                // create a wrapper and add it to the cards collection. Add the cards in a sorted order.
                foreach (Card card in cards.OrderBy((c) => c.CardValue))
                {
                    CardWrapper addition = new CardWrapper(card);
                    wrapper.Cards.Add(addition);
                    if (hand != null && hand.Contains(card))
                        addition.IsSelected = true;
                }
            }

        }


        #region IFiveCardClientHelper Members


        /// <summary>
        /// Called by the client when a drawing round starts
        /// </summary>
        public void NotifyDrawingRoundStarted()
        {
            Board.PostExclusiveMessage(Board.ThePlayer, "Waiting for draws.");
            // must respond so the helper will continue
            OnGuiClientResponded();
        }

        /// <summary>
        /// Called by the client to notify how many cards were drawn by the player
        /// </summary>
        /// <param name="player">The drawing players</param>
        /// <param name="drawCount">The amount of cards drawn. Can be 0</param>
        public void NotifyPlayerDraws(Player player, int drawCount)
        {
            CheckHasPlayer(player);
            string message = string.Empty;
            // get the drawing message
            if (drawCount == 0)
                message = "Stand pats";
            else if (drawCount == 1)
                message = "Draws one card";
            else
                message = string.Format("Draws {0} cards", drawCount);
            // set the message
            base[player].Message = message;
            LogLine(string.Format("{0} {1}", player.Name, message));
            if (player.Name == Board.ThePlayer.Name)
                // the player doesn't need to wait on the action perfromed by the user
                OnGuiClientResponded();
            else
                // wait a while so the user will have time to read the message
                Respond(TimeSpan.FromMilliseconds(1500));
        }

        /// <summary>
        /// Called by the client when a drawing round is finalized.
        /// </summary>
        public void FinalizeDrawingRound()
        {
            Board.ThePlayer.Message = "No more draws";
            LogLine("No more draws\n");
            // wait a while so the user will have time to read the message
            Respond(TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// Updates the current client with the new cards drawn.
        /// </summary>
        /// <param name="player">
        /// The player with the new updated cards
        /// </param>
        public void NotifyPlayerNewCards(Player player)
        {
            // replace the player cards using the helper callback
            UpdatePlayerCards(base[player], player.Cards);
            // wait a while so the user will have time to see the new cards
            Respond(TimeSpan.FromMilliseconds(1500));
        }


        /// <summary>
        /// Called by the client when a drawing round completes
        /// </summary>
        public void NotifyDrawingRoundCompleted()
        {
            OnGuiClientResponded();
        }


        #endregion
    }
}
