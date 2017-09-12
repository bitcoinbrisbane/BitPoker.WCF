using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace PokerService
{
    /// <summary>
    /// A class which holds poker server details
    /// </summary>
    [DataContract]
    public class ServerDetails
    {
        /// <summary>
        /// Gets or sets the number of players currently connected
        /// </summary>
        [DataMember]
        public int ConnectedPlayers { get; set; }
        /// <summary>
        /// Gets or sets a flag which indicates if the server accepts new connections
        /// </summary>
        [DataMember]
        public bool CanConnect { get; set; }
        /// <summary>
        /// Gets or sets the server running game
        /// </summary>
        [DataMember]
        public ServerGame Game { get; set; }

        /// <summary>
        /// Gets or sets the current hand played
        /// </summary>
        [DataMember]
        public int CurrentHand { get; set; }
    }
}
