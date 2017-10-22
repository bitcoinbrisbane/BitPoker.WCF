using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace PokerService
{
    /// <summary>
    /// An enumeration which indicates which game the server runs.
    /// </summary>
    [DataContract]
    public enum ServerGame
    {
        /// <summary>
        /// A five card draw game
        /// </summary>
        [EnumMember]
        FiveCardDraw,
        /// <summary>
        /// A texas hold'em game
        /// </summary>
        [EnumMember]
        TexasHoldem,
        /// <summary>
        /// A 7 card stud game
        /// </summary>
        [EnumMember]
        SevenCardStud,
        /// <summary>
        /// An omaha hold'em
        /// </summary>
        [EnumMember]
        OmahaHoldem
    }
}
