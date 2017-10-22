using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace PokerService
{
    /// <summary>
    /// A class which describes a service location
    /// </summary>
    public class ServiceLocation
    {
        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="ServiceLocation"/> class.</para>
        /// </summary>
        /// <param name="endpoint">The address of the server
        /// </param>
        public ServiceLocation(EndpointAddress endpoint)
        {
            Endpoint = endpoint;
        }
        
        /// <summary>
        /// Gets the endpoint in which the server listens
        /// </summary>
        public EndpointAddress Endpoint { get; private set; }

        /// <summary>
        /// Gets or sets the server details which is described with this location
        /// </summary>
        public ServerDetails ServerDetails { get; set; }
    }
}
