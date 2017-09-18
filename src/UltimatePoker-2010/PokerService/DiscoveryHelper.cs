using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Discovery;

namespace PokerService
{
    /// <summary>
    /// A helper class which can be used to discover poker games
    /// </summary>
    public class DiscoveryHelper
    {
        /// <summary>
        /// Tries to discover game servers in the LAN. This method blocks for at least two seconds.
        /// </summary>
        /// <returns>
        /// A collection of server responses which were accepted.
        /// </returns>
        public IEnumerable<ServiceLocation> Discover()
        {
            // create a new service finder 
            DiscoveryClient finder = new DiscoveryClient(new UdpDiscoveryEndpoint());
            
            // probe the network
            FindResponse found = finder.Find(new FindCriteria(typeof(IPokerHost)) { Duration = TimeSpan.FromSeconds(2), MaxResults = int.MaxValue });

            Collection<EndpointDiscoveryMetadata> endpoints = found.Endpoints;
            // the binding which is used to connect to each server
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            
            // the result array
            ServiceLocation[] location = new ServiceLocation[endpoints.Count];
            int i = 0;

            foreach (EndpointDiscoveryMetadata prop in endpoints)
            {
                // set the current result value
                location[i] = new ServiceLocation(prop.Address);
                // try to aquire server details
                try
                {
                    // create a channel to get the server details
                    IPokerHost host = ChannelFactory<IPokerHost>.CreateChannel(binding, prop.Address);
                    using (host as IDisposable)
                    {
                        location[i].ServerDetails = host.GetServerDetails();
                    }
                }
                catch
                {
                    // ignore misbehaved servers, return them as well
                }
                ++i;
            }
            return location;
        }
    }
}
