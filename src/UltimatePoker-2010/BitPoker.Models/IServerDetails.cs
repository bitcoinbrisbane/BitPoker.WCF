using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPoker.Models
{
    public interface IServerDetails
    {
        int ConnectedPlayers { get; set; }

        /// <summary>
        /// Gets or sets a flag which indicates if the server accepts new connections
        /// </summary>
        bool CanConnect { get; set; }

        /// <summary>
        /// Gets or sets the server running game
        /// </summary>
        //ServerGame Game { get; set; }

        /// <summary>
        /// Gets or sets the current hand played
        /// </summary>
        int CurrentHand { get; set; }
    }
}
