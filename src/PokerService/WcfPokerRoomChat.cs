using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using PokerEngine;

namespace PokerService
{
    /// <summary>
    /// The implementation of the <see cref="IPokerRoomChat"/> service
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WcfPokerRoomChat : IPokerRoomChat
    {
        // a mapping between chat sessions to poker sessions
        private Dictionary<string, string> chatSessionToPokerServiceSession = new Dictionary<string, string>();
        // a list of known clients
        private List<IPokerChatClient> clients = new List<IPokerChatClient>();
        // the concrete host which holds the poker service players
        private WcfEngineHelper concreteHost;

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="WcfPokerRoomChat"/> class.</para>
        /// </summary>
        /// <param name="concreteHost">The concrete host which holds the poker service players
        /// </param>
        public WcfPokerRoomChat(WcfEngineHelper concreteHost)
        {
            this.concreteHost = concreteHost;
        }

        /// <summary>
        /// Calls the given action on each of the known clients
        /// </summary>
        /// <param name="action">The action to perform</param>
        private void foreachClient(Action<IPokerChatClient> action)
        {
            IPokerChatClient[] copy = clients.ToArray();
            // go over all of the listeners
            foreach (IPokerChatClient cur in copy)
            {
                try
                {
                    action(cur);
                }
                catch
                {
                    dropListener(cur);
                }
            }
        }

        /// <summary>
        /// Disconnects the given client
        /// </summary>
        /// <param name="cur"></param>
        private void dropListener(IPokerChatClient cur)
        {
            // check if it is a known client
            if (clients.Remove(cur))
            {
                // disconnect the client
                ICommunicationObject comObject = cur as ICommunicationObject;
                if (comObject != null && comObject.State != CommunicationState.Closed)
                {
                    comObject.Abort();
                }
            }
        }

        #region IPokerRoomChat Members

        /// <summary>
        /// Tries to login the current client.
        /// </summary>
        /// <param name="pokerServiceSessionId">The session id of a valid session with the <see cref="IPokerService"/></param>
        /// <returns>True if the client logged in and can chat, false otherwise</returns>
        /// <remarks>
        /// The session id which is passed to <paramref name="pokerServiceSessionId"/> can be retrieved by the 
        /// <see cref="IContextChannel.SessionId"/> property.
        /// </remarks>
        public bool Login(string pokerServiceSessionId)
        {
            // Try to get the player associated with the given session id
            if (concreteHost.GetSafePlayer(pokerServiceSessionId) != null)
            {
                // found a proper client, add the current client
                clients.Add(OperationContext.Current.GetCallbackChannel<IPokerChatClient>());
                // map the current session to the poker session
                chatSessionToPokerServiceSession.Add(OperationContext.Current.SessionId, pokerServiceSessionId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Logouts the current client out of the chat room
        /// </summary>
        public void Logout()
        {
            clients.Remove(OperationContext.Current.GetCallbackChannel<IPokerChatClient>());
            chatSessionToPokerServiceSession.Remove(OperationContext.Current.SessionId);
        }

        /// <summary>
        /// Called by the client to speak on behalf of the user
        /// </summary>
        /// <param name="message">The user message</param>
        public void Speak(string message)
        {
            Player speaker = getCurrentSpeaker();
            if (speaker != null)
                foreachClient((cur) => cur.UserTalked(speaker, message));
        }

        #endregion

        /// <summary>
        /// Tries to get the current speaker
        /// </summary>
        /// <returns>The speaker, or null if none matches the current session</returns>
        private Player getCurrentSpeaker()
        {
            return concreteHost.GetSafePlayer(chatSessionToPokerServiceSession[OperationContext.Current.SessionId]);
        }
    }
}
