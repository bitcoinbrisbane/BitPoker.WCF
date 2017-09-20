using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using PokerEngine.Debug;
using System.Linq;

namespace PokerEngine.Betting
{
    /// <summary>
    /// This is the basic pot implementation. It provides capabilities for a regular pot.
    /// </summary>
    /// <remarks>
    /// This class does not support side pots, use <see cref="SidePot"/> for that.
    /// It implements <see cref="INotifyPropertyChanged"/> to allow GUI interaction.
    /// </remarks>
    public class Pot : INotifyPropertyChanged
    {
        // the set of players which participated in the last betting round. 
        private HashSet<Player> participatingPlayers = new HashSet<Player>();

        // A mapping between the player to their current bet. Cleared after ResetRaise
        private Dictionary<Player, int> playerToCurrentBet = new Dictionary<Player, int>();

        // trace players total bet.
        private Dictionary<Player, int> playerToTotalBet = new Dictionary<Player, int>();

        private int curRaise;

        /// <summary>
        /// Gets the current raise sum. The current raise can be raised by any player using a call to <see cref="Raise"/>
        /// </summary>
        public virtual int CurrentRaise
        {
            get
            {
                return (curRaise);
            }
            internal set
            {
                if (curRaise != value)
                {
                    curRaise = value;
                    OnPropertyChanged("CurrentRaise");
                }
            }
        }

        private int money;
        /// <summary>
        /// Gets tht total value of money in the pot.
        /// </summary>
        public virtual int Money
        {
            get
            {
                return (money);
            }
            internal set
            {
                if (money != value)
                {
                    money = value;
                    OnPropertyChanged("Money");
                }
            }
        }

        /// <summary>
        /// Can be called on behalf of any player, it raises the amount of the <see cref="CurrentRaise"/> by the given sum.
        /// </summary>
        /// <param name="player">Any player value, can't be unll</param>
        /// <param name="sum">Any positive value, this money is added to the <see cref="CurrentRaise"/></param>
        /// <remarks>
        /// Note that the player automatically calls, any previous debts to the pot and then raises the current bet.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Is thrown if the sum is not positive</exception>
        /// <exception cref="ArgumentException">Is thrown if the player doesn't have enough money</exception>
        public void Raise(Player player, int sum)
        {
            if (sum < 0)
                throw new ArgumentOutOfRangeException("sum", "ammount must be positive");
                
            if (player.Money < sum)
                throw new ArgumentException("player", "player does not have enough money");

            // increase the CurrentRaise value, Call(player) will collect the money
            CurrentRaise += sum;

            Call(player);
        }

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
        /// <exception cref="ArgumentException">Is thrown if the player doesn't have enough money</exception>
        public void Call(Player player)
        {
            // Get the player call sum
            int playerCall = GetPlayerCallSum(player);

            if (playerCall == 0) // skip 'check' handling
                return;
            if (player.Money < playerCall) // verify player has enough money
                throw new ArgumentException("player", "player does not have enough money");

            // add the player to the participating players set.
            participatingPlayers.Add(player);
            // set the player current bet
            playerToCurrentBet[player] = CurrentRaise;

            // trace the player bet
            if (!playerToTotalBet.ContainsKey(player))
                playerToTotalBet.Add(player, 0);

            playerToTotalBet[player] = playerToTotalBet[player] + playerCall;


            // set the player current bet
            player.CurrentBet += playerCall;
            // reduce the player money
            player.Money -= playerCall;
            // add the money to the pot.
            Money += playerCall;
        }

        /// <summary>
        /// Gets the player call sum. This is the amount of money the player needs to add to the pot
        /// </summary>
        /// <param name="player">Any player value, can't be null.
        /// </param>
        /// <returns>
        /// A sum indicates the amount of money the player needs to add to the pot so the player can participate in the winnings.
        /// </returns>
        public int GetPlayerCallSum(Player player)
        {
            // The player call is calculated using the (current raise - any bets the player has already made in the current round)
            int playerCall = CurrentRaise;
            if (playerToCurrentBet.ContainsKey(player)) // player has a bet value, use it
            {
                playerCall = CurrentRaise - playerToCurrentBet[player];
            }
            else // the player didn't contribute yet, add it with 0.
            {
                playerToCurrentBet.Add(player, 0);
            }
            return playerCall;
        }

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
        public void SplitPot(ICollection<Player> players)
        {
            // debug verifications
            Invariant.VerifyPotIsEven(this);
            Invariant.CheckPlayerSplit(this, players);

            // calculate the win amount per player
            int winAmount = Money / players.Count;
            foreach (Player player in players)
            {
                if (participatingPlayers.Contains(player)) // assure the player has participated in the bet
                    player.Money += winAmount;
                else
                    throw new InvalidOperationException("pot can be splitted only between participating players");
            }
            // if any remaineder exists, add it to the first player
            int remainder = Money % players.Count;
            players.First().Money += remainder;
            // Reset the pot data structures
            ResetPot();
        }

#if DEBUG
        /// <summary>
        /// Used for debug verifications. Gets each player total bet
        /// </summary>
        internal Dictionary<Player, int> PlayerToTotalBet { get { return playerToTotalBet; } }
#endif

        /// <summary>
        /// Gets the player total investment in the pot
        /// </summary>
        /// <param name="player">The player to check the amount of money invested</param>
        /// <returns>
        /// The player total bet amount in the current pot
        /// </returns>
        public int GetPlayerTotalBet(Player player)
        {
            int result;
            playerToTotalBet.TryGetValue(player, out result);
            return result;
        }

        /// <summary>
        /// Resets the pot state
        /// </summary>
        private void ResetPot()
        {
            // Call reset raise
            ResetRaise();
            Money = 0;
            // clear all dictionaries and sets
            participatingPlayers.Clear();
            playerToTotalBet.Clear();

        }

        /// <summary>
        /// Reset the pot raise, must be called after each betting round.
        /// </summary>
        public void ResetRaise()
        {
            // In debug mode, After each reset, assure the pot is even
            Invariant.VerifyPotIsEven(this);
            // Can clear current bets and raise.
            playerToCurrentBet.Clear();
            CurrentRaise = 0;
        }

        /// <summary>
        /// Notifies the pot that the given player has folded.
        /// </summary>
        /// <param name="player">Any player value, can't be null</param>
        public void Fold(Player player)
        {
            // the last player standing in the pot always wins the pot money...
            if (participatingPlayers.Count > 1)
                participatingPlayers.Remove(player);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event with the given property name.
        /// </summary>
        /// <param name="propName">The property name which was modified</param>
        protected void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        /// <summary>
        /// Determines if the given player can check.
        /// </summary>
        /// <param name="player">Any player value, can't be null</param>
        /// <returns>
        /// True if the player has participated in the pot and paid all of the required sum.
        /// </returns>
        public bool PlayerCanCheck(Player player)
        {
            return GetPlayerCallSum(player) == 0;
        }

        /// <summary>
        /// Gets a value indicating the the player has participated in the pot
        /// </summary>
        /// <param name="player">Any player value. Can't be null</param>
        /// <returns>
        /// True if the player has participated in the pot
        /// </returns>
        public bool HasPlayerParticipated(Player player)
        {
            return participatingPlayers.Contains(player);
        }

        /// <summary>
        /// Gets all of the pot participating players.
        /// </summary>
        public IEnumerable<Player> ParticipatedPlayers
        {
            get { return participatingPlayers; }
        }

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes. <see cref="INotifyPropertyChanged"/>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
