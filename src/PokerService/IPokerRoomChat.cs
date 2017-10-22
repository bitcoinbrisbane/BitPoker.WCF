using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace PokerService
{
    /// <summary>
    /// An interface which defines the poker chat room service.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IPokerChatClient), SessionMode = SessionMode.Required)]
    public interface IPokerRoomChat
    {
        /// <summary>
        /// Tries to login the current client.
        /// </summary>
        /// <param name="pokerServiceSessionId">The session id of a valid session with the <see cref="IPokerService"/></param>
        /// <returns>True if the client logged in and can chat, false otherwise</returns>
        /// <remarks>
        /// The session id which is passed to <paramref name="pokerServiceSessionId"/> can be retrieved by the 
        /// <see cref="IContextChannel.SessionId"/> property.
        /// </remarks>
        [OperationContract(IsInitiating = true)]
        bool Login(string pokerServiceSessionId);
        /// <summary>
        /// Logouts the current client out of the chat room
        /// </summary>
        [OperationContract(IsTerminating = true)]
        void Logout();

        /// <summary>
        /// Called by the client to speak on behalf of the user
        /// </summary>
        /// <param name="message">The user message</param>
        [OperationContract(IsOneWay = true)]
        void Speak(string message);
    }
}
