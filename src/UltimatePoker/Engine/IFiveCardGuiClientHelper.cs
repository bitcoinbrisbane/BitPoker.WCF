using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerConsole.Engine;

namespace UltimatePoker.Engine
{
    /// <summary>
    /// An interface which describes A gui helper which provides Five card draw capabilities as well
    /// </summary>
    public interface IFiveCardGuiClientHelper : IGuiClientHelper, IFiveCardClientHelper
    {
    }
}
