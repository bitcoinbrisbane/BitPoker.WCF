using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerRules.Games;
using PokerEngine;
using PokerEngine.Engine;
using PokerRules.Deck;
using System.Collections.ObjectModel;
using PokerService;
using PokerRules.Hands;

namespace PokerConsole.Engine
{
    /// <summary>
    /// Implementation of the Omaha Hold'em client
    /// </summary>
    public class OmahaHoldemClient : GameClient<OmahaHoldem>
    {
        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="OmahaHoldemClient"/> class.</para>
        /// </summary>
        /// <param name="helper">The helper to use. May be null but must manually set the <see cref="ClientHelperDecorator.Helper"/> property
        /// </param>
        public OmahaHoldemClient(IClientHelper helper)
            : base(helper)
        {
         
        }


        /// <summary>
        /// Called by the client when an update message arrives.
        /// </summary>
        /// <param name="player">The players sorted by their round order</param>
        /// <param name="potInformation">The current state of the pot</param>
        /// <param name="communityCards">The community cards in the game (if any) may be null or in 0 length</param>
        public override void WaitSynchronization(IEnumerable<Player> player, PotInformation potInformation, Card[] communityCards)
        {
            // updates the game community cards so player hands will be calculated correctly
            Game.ExposedCommunityCards = Array.AsReadOnly<Card>(communityCards);

            base.WaitSynchronization(player, potInformation, communityCards);

        }

    }

}
