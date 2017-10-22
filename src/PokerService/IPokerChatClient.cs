using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using PokerEngine;

namespace PokerService
{
    /// <summary>
    /// The callback service contract which is used by the <see cref="IPokerRoomChat"/> service
    /// </summary>
    public interface IPokerChatClient
    {
        /// <summary>
        /// Called by the service when a client talks.
        /// </summary>
        /// <param name="user">The player which talked</param>
        /// <param name="message">The player message</param>
        [OperationContract(IsOneWay = true)]
        void UserTalked(Player user, string message);
    }
}
