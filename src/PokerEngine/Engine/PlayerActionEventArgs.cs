using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerEngine.Engine
{
    /// <summary>
    /// A class which defines event arguments for a player action
    /// </summary>
    public class PlayerActionEventArgs : EventArgs
    {
        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="PlayerActionEventArgs"/> class.</para>
        /// </summary>
        /// <param name="player">The player which performed the action</param>
        /// <param name="betAction">The action performed by the player</param>
        /// <param name="callAmount">The player call amount (can be 0)</param>
        /// <param name="raiseAmount">The player raise amount (can be 0)</param>
        /// <param name="isBlindAction">A flag indicating if the action was made by the player or was a blind action</param>
        public PlayerActionEventArgs(Player player, BetAction betAction, int callAmount, int raiseAmount, bool isBlindAction)
        {
            Player = player;
            CallAmount = callAmount;
            RaiseAmount = raiseAmount;
            IsBlindAction = isBlindAction;
            PlayerAction = betAction;
        }
        /// <summary>
        /// Gets the player which performed the action
        /// </summary>
        public Player Player { get; private set; }

        /// <summary>
        /// Gets the player call amount (can be 0)
        /// </summary>
        public int CallAmount { get; private set; }

        /// <summary>
        /// Gets the player raise amount (can be 0)
        /// </summary>
        public int RaiseAmount { get; private set; }

        /// <summary>
        /// Gets a flag indicating if the action was made by the player or was a blind action
        /// </summary>
        public bool IsBlindAction { get; private set; }

        /// <summary>
        /// Gets the action performed by the player
        /// </summary>
        public BetAction PlayerAction { get; private set; }
    }
}
