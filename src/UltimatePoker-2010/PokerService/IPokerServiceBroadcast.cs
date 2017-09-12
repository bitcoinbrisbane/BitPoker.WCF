using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace PokerService
{
    /// <summary>
    /// An interface which describes the poker broadcast service
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IPokerClient))]
    [DeliveryRequirements(RequireOrderedDelivery = true)]
    public interface IPokerServiceBroadcast
    {
        /// <summary>
        /// Requests permissions to spectate the running game
        /// </summary>
        /// <returns>
        /// True if the client can watch, false otherwise.
        /// </returns>
        [OperationContract]
        bool RequestSpectation();
    }
}
