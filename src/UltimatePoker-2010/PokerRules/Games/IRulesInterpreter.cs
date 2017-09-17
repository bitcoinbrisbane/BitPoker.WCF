using System;
using System.Collections.Generic;
using BitPoker.Models.Deck;
using BitPoker.Models.Hands;

namespace PokerRules.Games
{
    /// <summary>
    /// This interface describes a class which can provide game rules interpertation
    /// </summary>
    public interface IRulesInterpreter
    {
        /// <summary>
        /// Gets the best hand according to this game logic out of the given cards.
        /// </summary>
        /// <param name="cards">A collection of cards to search for the best hand. Must not be null.</param>
        /// <returns>
        /// The best hand which can be creates using the given cards or null if none exists.
        /// </returns>
        Hand GetBestHand(IEnumerable<Card> cards);
    }
}
