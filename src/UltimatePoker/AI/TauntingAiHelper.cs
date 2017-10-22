using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PokerConsole.Engine;

namespace UltimatePoker.AI
{
    public class TauntingAiHelper : ClientHelperDecorator
    {
        private string name;
        private BaseWcfClient client;
        public TauntingAiHelper(IClientHelper helper)
            : base(helper)
        {

        }
        public void Initialize(BaseWcfClient client)
        {
            this.client = client;
        }

        public override string GetName()
        {
            name = base.GetName();
            return name;
        }

        public override void NotifyPlayerStatus(PokerEngine.Player player, bool isWinner)
        {
            base.NotifyPlayerStatus(player, isWinner);
            if (player.Name == name)
            {
                if (isWinner)
                    Speak("Come to papa");
                else
                    Speak("Do you accept checks?");
            }

        }

        public override void WaitPlayerBettingAction(PokerEngine.Player player, PokerEngine.Engine.PlayerBettingAction action)
        {
            base.WaitPlayerBettingAction(player, action);
            switch (action.Action)
            {
                case PokerEngine.Engine.BetAction.CheckOrCall: repondOnCall(action); break;
                case PokerEngine.Engine.BetAction.Fold: respondOnFold(action); break;
                case PokerEngine.Engine.BetAction.Raise: respondOnRaise(action); break;
            }
        }

        private void repondOnCall(PokerEngine.Engine.PlayerBettingAction action)
        {
            if (action.CallAmount == 0)
            {
                Speak("I'll take a free card");
            }
            else
            {
                Speak("Call. I want to see how it goes");
            }
        }

        private void respondOnFold(PokerEngine.Engine.PlayerBettingAction action)
        {
            Speak("Call! I mean fold...");
        }

        private void respondOnRaise(PokerEngine.Engine.PlayerBettingAction action)
        {
            Speak("Raise!, you might as well fold");
        }

        protected void Speak(string message)
        {
            client.PokerRoomChat.Speak(message);
        }
    }
}
