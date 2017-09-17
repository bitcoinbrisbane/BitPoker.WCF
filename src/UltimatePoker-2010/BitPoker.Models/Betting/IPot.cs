using System;
using System.Collections.Generic;

namespace BitPoker.Models.Betting
{
    /// <summary>
    /// The basic interface of the pot.
    /// </summary>
    public interface IPot
    {
        /// <summary>
        /// Notifies the pot the given player has called.
        /// </summary>
        /// <param name="player">A player which hasn't folded yet. Can't be null
        /// </param>
        /// <remarks>
        /// When a player calls, it evens the player bet to the <see cref="CurrentRaise"/>. If the player doesn't have enough money,
        /// all of it's money is taken and a side pot should be used for the rest of the players. 
        /// A player can't win the amount of money the player didn't contributed to the pot.
        /// </remarks>
        void Call(IPlayer player);

        /// <summary>
        /// Gets the current raise sum. The current raise can be raised by any player using a call to <see cref="Raise"/>
        /// </summary>
        UInt64 CurrentRaise { get; }

        /// <summary>
        /// Gets the player call sum. This is the amount of money the player needs to add to the pot
        /// </summary>
        /// <param name="player">Any player value, can't be null.
        /// </param>
        /// <returns>
        /// A sum indicates the amount of money the player needs to add to the pot so the player can participate in the winnings.
        /// </returns>
        UInt64 GetPlayerCallSum(IPlayer player);

        /// <summary>
        /// Gets tht total value of money in the pot.
        /// </summary>
        UInt64 Money { get; }

        /// <summary>
        /// Notifies the pot that the given player has folded.
        /// </summary>
        /// <param name="player">Any player value, can't be null</param>
        void Fold(IPlayer player);

        /// <summary>
        /// Determines if the given player can check.
        /// </summary>
        /// <param name="player">Any player value, can't be null</param>
        /// <returns>
        /// True if the player has participated in the pot and paid all of the required sum.
        /// </returns>
        bool PlayerCanCheck(IPlayer player);

        /// <summary>
        /// Can be called on behalf of any player, it raises the amount of the <see cref="CurrentRaise"/> by the given sum.
        /// </summary>
        /// <param name="player">Any player value, can't be unll</param>
        /// <param name="sum">Any positive value, this money is added to the <see cref="CurrentRaise"/></param>
        /// <remarks>
        /// Note that the player automatically calls, any previous debts to the pot and then raises the current bet.
        /// </remarks>
        void Raise(IPlayer player, UInt64 sum);

        /// <summary>
        /// Reset the pot raise, must be called after each betting round.
        /// </summary>
        void ResetRaise();

        /// <summary>
        /// Splits the pot between the winning players.
        /// </summary>
        /// <param name="players">
        /// A collection of winners, must not be null and contain at least 1 winner.
        /// </param>
        /// <remarks>
        /// The winners must all contribute evenly to the pot so they can split it. 
        /// A player can't earn money from the pot when the player didn't participate in the bettings.
        /// </remarks>
        void SplitPot(ICollection<IPlayer> players);
    }
}
