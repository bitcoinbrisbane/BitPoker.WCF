using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using PokerEngine.Betting;
using System.Runtime.Serialization;

namespace PokerService
{
    /// <summary>
    /// A class which holds the current state of the pot
    /// </summary>
    [DataContract]
    public class PotInformation
    {

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="PotInformation"/> class.</para>
        /// </summary>
        /// <param name="potAmount">The current amount of money in the pot
        /// </param>
        /// <param name="potData">The table which holds each player investment in each side pot
        /// </param>
        public PotInformation(int potAmount, int[][] potData)
        {
            PotAmount = potAmount;
            SidePotsInformation = potData;
            
        }
        /// <summary>
        /// Gets the current amount of money in the pot
        /// </summary>
        [DataMember]
        public int PotAmount { get; private set; }

        /// <summary>
        /// Gets the table which holds each player investment in each side pot.
        /// </summary>
        /// <remarks>
        /// The structure of the table is desribed by <see cref="SidePot.GetPlayersBettingData"/>
        /// </remarks>
        [DataMember]
        public int[][] SidePotsInformation { get; private set; }
    }
}
