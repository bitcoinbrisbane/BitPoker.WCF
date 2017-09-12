using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerRules.Games;

namespace PokerEngine.Engine
{
    /// <summary>
    /// The basic engine which manages Omaha Hold'em game logic.
    /// </summary>
    /// <remarks>
    /// Omaha Hold'em is a variation of Texas hold'em where each hand must be composed out of exactly 2 cards 
    /// out of the Hole cards, and exactly 3 cards out of the community cards.
    /// </remarks>
    public abstract class BaseOmahaHoldemEngine : BaseTexasHoldemEngine
    {
        /// <summary>
        /// Creates a new instance of the <see cref="OmahaHoldem"/> game.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="OmahaHoldem"/> game.
        /// </returns>
        protected override TexasHoldem GetNewGameOverride()
        {
            return new OmahaHoldem();
        }
    }
}
