using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using PokerEngine.Debug;
using System.Linq;

namespace PokerEngine.Betting
{
    /// <summary>
    /// An implemenation of a pot which can be splitted to side pots in case a player runs out of money.
    /// </summary>
    public class SidePot
    {
        // The list of currently active pots. This holds the pots which has active raises in them.
        private List<Pot> activePots = new List<Pot>();
        // The list of all pots
        private List<Pot> pots = new List<Pot>();
        // The current pot (activePots[activePots.Count - 1])
        private Pot currentPot;
        // A list of players which is used to test the pot split, players 
        // are removed when a player folds or when the player caused a split
        private List<Player> roundPlayers = new List<Player>();
        /// <summary>
        /// Creates a new instance of the SidePot
        /// </summary>
        public SidePot(IEnumerable<Player> roundPlayers)
        {
            this.roundPlayers = new List<Player>(roundPlayers);
            NewPot();
        }

        /// <summary>
        /// Creates a new pot and adds it to the managing collections
        /// </summary>
        private void NewPot()
        {
            // create a new empty pot
            currentPot = new Pot();
            // add it to both the activePots and the pots lists
            pots.Add(currentPot);
            activePots.Add(currentPot);
        }

        #region IPot Members

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
        public void Call(Player player)
        {
            // The player needs to call in each of the active pots in order to fully repay the player debt
            foreach (Pot pot in activePots)
            {
                // verify that this pot is suitalbe for the player. The player can't call a pot which 
                // has a higher raise amount than the player money.
                if (pot.GetPlayerCallSum(player) <= player.Money)
                {
                    pot.Call(player);
                }
                else // in debug mode, verity that the player didn't participate in a pot which the player can call.
                {
                    Invariant.VerifyPlayerParticipation(pot, player);
                }
            }
        }

        /// <summary>
        /// Gets the pot current raise
        /// </summary>
        public int CurrentRaise
        {
            // calculate the current raise of all of the active pots.
            get { return activePots.Sum((pot) => pot.CurrentRaise); }
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
            // sum over all of the active pots call sums
            return activePots.Sum((pot) => pot.GetPlayerCallSum(player));
        }

        /// <summary>
        /// Gets tht total value of money in the pot.
        /// </summary>
        public int Money
        {
            get
            {
                // sum over all of the pots money
                return pots.Sum((pot) => pot.Money);
            }
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
            return currentPot.PlayerCanCheck(player);
        }

        /// <summary>
        /// Can be called on behalf of any player, it raises the amount of the <see cref="CurrentRaise"/> by the given sum.
        /// </summary>
        /// <param name="player">Any player value, can't be unll</param>
        /// <param name="sum">Any positive value, this money is added to the <see cref="CurrentRaise"/></param>
        /// <remarks>
        /// Note that the player automatically calls, any previous debts to the pot and then raises the current bet.
        /// </remarks>
        public void Raise(Player player, int sum)
        {
            // first call the player debts to the pots.
            Call(player);
            // calcualate true raise sum after splitting the pots as it should
            int trueRaise = CheckSplit(player, sum);
            // raise the current pot with the true raise sum
            currentPot.Raise(player, trueRaise);

        }

        /// <summary>
        /// Creates side pots as needed and raises the bet in each side pot according to the given sum
        /// </summary>
        /// <param name="player">The player which raises the bet</param>
        /// <param name="sum">The sum to increase the bet</param>
        /// <returns>
        /// A new sum which remains after each side pot already increased the bet. This sum needs to be raised in the currentPot.
        /// </returns>
        private int CheckSplit(Player player, int sum)
        {
            // The pot splitting calculation creates a pot for each player with a lower amount of money.
            // For example:
            // Assume 3 players: a, b and c.
            // a has 3000$, b has 2000$ and c has 1000$.

            // In the case a raises 3000$:
            // The first pot will have a bet of 1000$ so c could join,
            // The second pot will have a bet of 1000$ as well, b will have to call the first pot 1000$ and the second pot 1000$
            // so b will contribute 2000$
            // The third pot will have a bet of 1000$ and only c will participate in it. The money in there will return the c eventually.

            // In the case b raises 2000$:
            // The first pot will have 1000$ so c could join
            // The second pot will have 1000$ as well, so a could join.

            // sort the money values, keep each value fisrt as both the key and the value.
            // The values will be adjusted to the differentiated sums.
            SortedList<int, int> sortedMoney = new SortedList<int, int>();
            // a temporary list used to store the players and their real money amount
            List<KeyValuePair<Player, int>> playerRealMoney = new List<KeyValuePair<Player, int>>();
            foreach (Player curPlayer in roundPlayers)
            {
                // the player actual money is calculated with the CallSum
                int realMoney = curPlayer.Money - GetPlayerCallSum(curPlayer);
                // negative amounts indicates that the player is already "all in"
                if (realMoney >= 0)
                {
                    if (!sortedMoney.ContainsKey(realMoney)) // no need for duplicate values
                        sortedMoney.Add(realMoney, realMoney);

                    playerRealMoney.Add(new KeyValuePair<Player, int>(curPlayer, realMoney));
                }
            }

            // need to split only if exists a player with low amount of money. The player real money should be lower than the call sum.
            while (sortedMoney.Count > 0 && sortedMoney.Values[0] < sum)
            {
                // raise the current pot with the minimal amount of money.
                int minimalKey = sortedMoney.Keys[0];
                int minimalMoney = sortedMoney.Values[0];
                currentPot.Raise(player, minimalMoney);
                // create a new pot
                NewPot();
                // reduce the raised amount of the total raise sum
                sum -= minimalMoney;
                // remove the lowest value which was raised
                sortedMoney.Remove(minimalKey);
                // remove all players which caused the current split:
                //   remove cur from roundPlayer
                //   where cur.realMoney == minimalKey 
                roundPlayers.RemoveAll((cur) => playerRealMoney.Exists((pair) => pair.Key == cur && pair.Value == minimalKey));
                if (sortedMoney.Count > 0)
                {
                    // reduce all of the values with the current raised sum.
                    // need to copy the keys since a change to the values will screw up the interator
                    int[] keys = new int[sortedMoney.Count];
                    sortedMoney.Keys.CopyTo(keys, 0);
                    foreach (int key in keys)
                    {
                        sortedMoney[key] = sortedMoney[key] - minimalMoney;
                    }
                }
            }
            return sum;
        }

        /// <summary>
        /// Reset the pot raise, must be called after each betting round.
        /// </summary>
        public void ResetRaise()
        {
            // reset the raise in the active pots.
            foreach (Pot pot in activePots)
            {
                pot.ResetRaise();
            }
            activePots.Clear();
            // assure the active pots always contains the current pot.
            activePots.Add(currentPot);


        }
        /// <summary>
        /// Notifies the pot that the given player has folded.
        /// </summary>
        /// <param name="player">Any player value, can't be null</param>
        public void Fold(Player player)
        {
            roundPlayers.Remove(player);
            // set the player as folds in each of the pots
            foreach (Pot pot in pots)
                pot.Fold(player);
        }


        /// <summary>
        /// Splits the pot between the winning players.
        /// </summary>
        /// <param name="comparer">A comparer which ranks the player to split the money to</param>
        /// <remarks>
        /// The winners must all contribute evenly to the pot so they can split it. 
        /// A player can't earn money from the pot when the player didn't participate in the bettings.
        /// The comparer is used to determine between winners in side pots, it should rank the players according to their hand/fold value
        /// </remarks>
        public void SplitPot(Comparison<Player> comparer)
        {
            // clone will contain the winners or the current pot participating players.
            List<Player> clone = new List<Player>();
            foreach (Pot pot in pots)
            {
                // clear the clone.
                clone.Clear();
                // add the participating players to the clone
                clone.AddRange(pot.ParticipatedPlayers);
                // use the given compraer to sort them
                clone.Sort(comparer);
                // select the top most winners (select the last since the sort is ascending)
                Player topMost = clone[clone.Count - 1];
                // remove all lower ranked players
                clone.RemoveAll((cur) => comparer(topMost, cur) > 0);
                // split the earnings of the current pot
                pot.SplitPot(clone);
            }
            // Reset the current pot, clearing all data structures.
            ResetPot();
        }

        /// <summary>
        /// Clears the pot state.
        /// </summary>
        private void ResetPot()
        {
            ResetRaise();
            pots.Clear();
            NewPot();
        }

        #endregion

#if DEBUG
        /// <summary>
        /// For debugging purposes, calcaulate the player total bet sum.
        /// </summary>
        /// <param name="player">Any player value</param>
        /// <returns>The sum of the player bets</returns>
        internal int GetPlayerTotalBet(Player player)
        {
            return pots.Sum(
                delegate(Pot cur)
                {
                    int result;
                    cur.PlayerToTotalBet.TryGetValue(player, out result);
                    return result;
                });
        }
#endif

        /// <summary>
        /// Gets a 2 dimensional table of the players investment in the pots.
        /// </summary>
        /// <param name="players">The player to get the pot investment. Must not be null</param>
        /// <returns>
        /// A 2 dimensional table of the players investment in the pots. The first dimension holds a row for each player.
        /// In each of that rows, the player investment in all of the pots is described.
        /// </returns>
        public int[][] GetPlayersBettingData(IEnumerable<Player> players)
        {
            // order the pots by the total amount of money invested
            IEnumerable<Pot> orderedPots = from p in pots
                                           orderby p.Money descending
                                           select p;
            int playersCount = players.Count();
            int potsCount = pots.Count; ;
            // create the result table
            int[][] result = new int[playersCount][];
            // initialize counters for player dimension
            int i = 0;
            foreach (Player curPlayer in players)
            {
                result[i] = new int[potsCount];
                // initialize counters for pots dimension
                int j = 0;
                foreach (Pot pot in orderedPots)
                {
                    // set the current table cell data:
                    result[i][j] = pot.GetPlayerTotalBet(curPlayer);
                    ++j;
                }
                ++i;
            }
            return result;
        }
    }
}
