using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace PokerService
{
    /// <summary>
    /// A wrapping service implementation of the <see cref="IPokerServiceBroadcast"/> using the <see cref="WcfEngineHelper"/>
    /// implementation
    /// </summary>
    
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WcfPokerBroadcast : IPokerServiceBroadcast
    {
        private WcfEngineHelper concreteHelper;

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="WcfPokerBroadcast"/> class.</para>
        /// </summary>
        /// <param name="concreteHelper">The helper which runs the game
        /// </param>
        public WcfPokerBroadcast(WcfEngineHelper concreteHelper)
        {
            this.concreteHelper = concreteHelper;
        }

        #region IPokerServiceBroadcast Members

        /// <summary>
        /// Requests permissions to spectate the running game
        /// </summary>
        /// <returns>
        /// True if the client can watch, false otherwise.
        /// </returns>
        public bool RequestSpectation()
        {
            return concreteHelper.RequestSpectation();
        }

        #endregion
    }
}
