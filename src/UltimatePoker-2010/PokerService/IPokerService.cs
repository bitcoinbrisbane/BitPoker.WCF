using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using PokerEngine;
using PokerEngine.Engine;

namespace PokerService
{
    /// <summary>
    /// The interface which describes the poker service
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IPokerClient), SessionMode = SessionMode.Required)]
    [DeliveryRequirements(RequireOrderedDelivery = true)]
    public interface IPokerService
    {
        /// <summary>
        /// Tries to login the client with the given user name.
        /// </summary>
        /// <param name="userName">The name to sign in the client</param>
        /// <returns>
        /// A valid player when the sign in succeeds. Null value is returned in case of an error (after a proper callback)
        /// </returns>
        /// <remarks>
        /// Either <see cref="IPokerClient.NotifyNameExists"/> or <see cref="IPokerClient.NotifyGameInProgress"/> is called in case 
        /// of an error.
        /// </remarks>
        [OperationContract(IsInitiating = true)]
        Player Login(string userName);

        /// <summary>
        /// Request the server to logout from the current game
        /// </summary>
        [OperationContract(IsTerminating = true)]
        void Logout();

        /// <summary>
        /// Gets the player current action deadline. MUST be called by the player which needs to make a move
        /// </summary>
        /// <returns>
        /// A time span which indicates the remaining time for the player action, or <see cref="TimeSpan.Zero"/>
        /// </returns>
        /// <remarks>
        /// When there is no deadline or when it isn't the calling client turn, a value of <see cref="TimeSpan.Zero"/> is returned.
        /// The client must repond with in the given time span.
        /// </remarks>
        [OperationContract()]
        TimeSpan GetCurrentActionDeadLine();

        
    }
}
