using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Net.Security;
using PokerEngine;

namespace PokerService
{
    /// <summary>
    /// The poker host service
    /// </summary>
    [ServiceContract]
    public interface IPokerHost
    {
        /// <summary>
        /// Gets the host details which runs the game
        /// </summary>
        /// <returns>
        /// The running game details
        /// </returns>
        [OperationContract]
        ServerDetails GetServerDetails();
        /// <summary>
        /// Gets a list of players which are connected to the game
        /// </summary>
        /// <returns>A list of players which are connected to the game</returns>
        [OperationContract]
        IEnumerable<Player> GetLoggedinPlayers();
        
    }
}
