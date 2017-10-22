using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerConsole.Engine;

namespace UltimatePoker.AI
{
    public class TauntinAiFiveCardHelper : TauntingAiHelper, IFiveCardClientHelper
    {
        private IFiveCardClientHelper helper;
        public TauntinAiFiveCardHelper(IFiveCardClientHelper helper)
            : base(helper)
        {
            this.helper = helper;
        }

        #region IFiveCardClientHelper Members

        public void NotifyDrawingRoundStarted()
        {
            helper.NotifyDrawingRoundStarted();
        }

        public void NotifyDrawingRoundCompleted()
        {
            helper.NotifyDrawingRoundCompleted();
        }

        public void NotifyPlayerDraws(PokerEngine.Player player, int drawCount)
        {
            helper.NotifyPlayerDraws(player, drawCount);
        }

        public void NotifyPlayerNewCards(PokerEngine.Player player)
        {
            helper.NotifyPlayerNewCards(player);
        }

        public void WaitPlayerDrawingAction(PokerEngine.Player player, PokerEngine.Engine.PlayerDrawingAction action)
        {
            helper.WaitPlayerDrawingAction(player, action);
            if (action.DrawnCards.Count == 1)
            {
                Speak("All I need is just one card...");
            }
            else if (action.DrawnCards.Count == 0)
            {
                Speak("You might as well fold. I don't need any card!");
            }
        }

        #endregion
    }
}
