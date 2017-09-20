using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerRules.Games;
using BitPoker.Models.Hands;
using BitPoker.Models.Deck;

namespace PokerConsole.Engine
{
    /// <summary>
    /// A client helper wrapper which enables rules interpretation based on poker games
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GameClient<T> : ClientHelperDecorator, IRulesInterpreter where T : BaseGame, new()
    {
        // The game instance
        private T game;

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="GameClient{T}"/> class.</para>
        /// </summary>
        /// <param name="helper">The real helper to use
        /// </param>
        public GameClient(IClientHelper helper)
            : base(helper)
        {
            // create and start the game
            game = new T();
            game.BeginGame(1);
        }

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
            return game.GetBestHand(cards);
        }

        #endregion

        /// <summary>
        /// Gets the created game
        /// </summary>
        protected T Game { get { return game; } }
    }
}
