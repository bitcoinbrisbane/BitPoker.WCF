using System;
using System.Collections.Generic;
using System.Text;
using PokerConsole.Engine;
using PokerEngine;
using BitPoker.Models.Deck;
using BitPoker.Models.Hands;
using PokerRules.Games;

namespace UltimatePoker.Engine
{
    /// <summary>
    /// A simple Gui client of the Texas Hold'em game.
    /// </summary>
    public class TexasHoldemGuiClient : ConcreteHelper
    {

        // The number of "hole" cards
        private int privateCardsCount;// texas hold'em has 2 private "hole" cards

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="TexasHoldemGuiClient"/> class.</para>
        /// </summary>
        /// <param name="initialName">The initial user name which is the login name of the player
        /// </param>
        /// <param name="client">The client which runs the game, it is used to get the player hands
        /// </param>
        public TexasHoldemGuiClient(string initialName, IRulesInterpreter client, int privateCardsCount)
            : base(initialName, client)
        {
            this.privateCardsCount = privateCardsCount;
        }
        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="TexasHoldemGuiClient"/> class.</para>
        /// </summary>
        /// <param name="initialName">The initial user name which is the login name of the player
        /// </param>
        /// <param name="client">The client which runs the game, it is used to get the player hands
        /// </param>
        public TexasHoldemGuiClient(IRulesInterpreter client, int privateCardsCount)
            : base(client)
        {
            this.privateCardsCount = privateCardsCount;
        }

        protected override void UpdatePlayerCards(GuiPlayerWrapper wrapper, IEnumerable<Card> cards)
        {
            // clear existing cards
            wrapper.Cards.Clear();
            // a counter which counts the number of cards to display
            int index = 0;

            if (cards != null)
            {
                IEnumerator<Card> enumerator = cards.GetEnumerator();
                // make sure only the "hole" cards are displayed
                while (enumerator.MoveNext() && index < privateCardsCount)
                {
                    Card card = enumerator.Current;
                    wrapper.Cards.Add(new CardWrapper(card));
                    ++index;
                }

                // Get the player hand and update it
                Hand hand = Client.GetBestHand(cards);
                if (hand != null)
                {
                    wrapper.CurrentHand = hand;
                    Board.SelectExclusiveCards(new List<Card>(hand));
                }
            }

        }

        /// <summary>
        /// Called by the client when an update message arrives.
        /// </summary>
        /// <param name="player">The players sorted by their round order</param>
        /// <param name="potInformation">The current state of the pot</param>
        /// <param name="communityCards">The community cards in the game (if any) may be null or in 0 length</param>
        public override void WaitSynchronization(IEnumerable<Player> player, PokerService.PotInformation potInformation, Card[] communityCards)
        {
            // clear existing cards
            Board.CommunityCards.Clear();
            // add the new ones
            foreach (Card card in communityCards)
            {
                Board.CommunityCards.Add(new CardWrapper(card));
            }
            base.WaitSynchronization(player, potInformation, communityCards);
        }
    }
}
