using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using PokerEngine;

namespace PokerService
{
    /// <summary>
    /// A service wrapper which implements the <see cref="IPokerHost"/> using a <see cref="WcfEngineHelper"/>
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WcfPokerHost : IPokerHost
    {
        private WcfEngineHelper concreteHost;

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="WcfPokerHost"/> class.</para>
        /// </summary>
        /// <param name="concreteHost">The wcf helper which actually provides the service implementation
        /// </param>
        public WcfPokerHost(WcfEngineHelper concreteHost)
        {
            this.concreteHost = concreteHost;
        }

        #region IPokerHost Members

        /// <summary>
        /// Gets the host details which runs the game
        /// </summary>
        /// <returns>
        /// The running game details
        /// </returns>
        public ServerDetails GetServerDetails()
        {
            return concreteHost.GetServerDetails();
        }

        /// <summary>
        /// Gets a list of players which are connected to the game
        /// </summary>
        /// <returns>A list of players which are connected to the game</returns>
        public IEnumerable<Player> GetLoggedinPlayers()
        {
            return concreteHost.GetLoggedinPlayers();
        }

        #endregion
    }
}
