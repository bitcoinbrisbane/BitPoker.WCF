using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerRules.Games;
using BitPoker.Models.Deck;
using BitPoker.Models.Hands;

namespace PokerConsole.Engine
{
    /// <summary>
    /// A bridge which is used to replace rules interpreters
    /// </summary>
    /// <remarks>
    /// Using classes must set a proper value for the <see cref="Interpreter"/> property.
    /// </remarks>
    public class RulesInterpreterBridge : IRulesInterpreter
    {
        /// <summary>
        /// Gets or sets the used interpreter
        /// </summary>
        public IRulesInterpreter Interpreter { get; set; }

        #region IRulesInterpreter Members

        /// <summary>
        /// Gets the best hand according to this game logic out of the given cards.
        /// </summary>
        /// <param name="cards">A collection of cards to search for the best hand. Must not be null.</param>
        /// <returns>
        /// The best hand which can be creates using the given cards or null if none exists.
        /// </returns>
        public Hand GetBestHand(IEnumerable<Card> cards)
        {
            return Interpreter.GetBestHand(cards);
        }

        #endregion
    }
}
