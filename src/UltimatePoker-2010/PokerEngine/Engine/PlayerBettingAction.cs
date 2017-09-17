using System;
using System.Collections.Generic;
using System.Text;

namespace PokerEngine.Engine
{
    /// <summary>
    /// The possible bet actions
    /// </summary>
    public enum BetAction
    {
        /// <summary>
        /// The player checks or calsl
        /// </summary>
        CheckOrCall,
        /// <summary>
        /// The player raises
        /// </summary>
        Raise,
        /// <summary>
        /// The player folds
        /// </summary>
        Fold
    }

    /// <summary>
    /// A class which represents a player betting action.
    /// </summary>
    /// <remarks>
    /// The game engine uses this action to communicate with the player. The action holds some restrictions and limitations.
    /// </remarks>
    [Serializable]
    public class PlayerBettingAction
    {
        
        /// <summary>
        /// Creates a new instance of the PlayerBettingAction class
        /// </summary>
        /// <param name="callAmount">The player call amount</param>
        /// <param name="smallRaise">The small raise limit</param>
        /// <param name="isAllInMode">A flag which indicates if the action is an "all in action"</param>
        /// <param name="canRaise">A flag which indicates if the player can raise or just call</param>
        public PlayerBettingAction(int callAmount, int smallRaise, bool isAllInMode, bool canRaise)
        {
            this.isAllInMode = isAllInMode;
            this.callAmount = callAmount;
            this.raiseAmount = smallRaise;
            CanRaise = canRaise;
        }

        private BetAction action = BetAction.Fold;
        /// <summary>
        /// Gets a value indicating if the player can raise. 
        /// </summary>
        /// <remarks>
        /// Some games have a raise limit that does not allow infinite raise actions.
        /// </remarks>
        public bool CanRaise { get; private set; }

        /// <summary>
        /// Gets the current bet action.
        /// </summary>
        public BetAction Action
        {
            get { return action; }
        }

        private int raiseAmount = 0;
        /// <summary>
        /// Gets the current raise amount. This initially holds the Small Raise of the game.
        /// </summary>
        /// <remarks>
        /// Some games limit their raise amount to a minimal sum. A player won't be able to raise an amount lower than that
        /// </remarks>
        public int RaiseAmount
        {
            get { return raiseAmount; }
        }

        private bool isAllInMode;
        /// <summary>
        /// Gets a value indicating if the bet action is an "all in" action.
        /// </summary>
        /// <remarks>
        /// "All in" actions are usually when the current raise amount is higher than the player money.
        /// Some tournament may decide that after a certain point all bets are "all in" forcing the game to a decision point.
        /// </remarks>
        public bool IsAllInMode
        {
            get { return isAllInMode; }
        }

        private int callAmount;
        /// <summary>
        /// Gets the player call amount. This is the amount the player will have to add in order to remain in the current round
        /// </summary>
        public int CallAmount
        {
            get { return callAmount; }
        }

        /// <summary>
        /// Marks this action as a Call action.
        /// </summary>
        public void Call()
        {
            action = BetAction.CheckOrCall;
        }

        /// <summary>
        /// Marks this action as a Raise action with the given amount.
        /// </summary>
        /// <param name="raiseAmount">The amount to raise in addition to the <see cref="CallAmount"/></param>
        /// <remarks>
        /// If the raise amount is negative, the action will fold.
        /// If the action is marked with the flag <see cref="CanRaise"/> as false, the action will call.
        /// </remarks>
        public void Raise(int raiseAmount)
        {
            this.raiseAmount = raiseAmount;
            // check if the action can be raised or if the raiseAmount is 0
            if (!CanRaise || raiseAmount == 0)
                Call();
            else if (raiseAmount < 0) // can't raise with negative values
                Fold();
            else
                action = BetAction.Raise;
        }

        /// <summary>
        /// Marks this action as a Fold action.
        /// </summary>
        public void Fold()
        {
            action = BetAction.Fold;
        }

    }
}
