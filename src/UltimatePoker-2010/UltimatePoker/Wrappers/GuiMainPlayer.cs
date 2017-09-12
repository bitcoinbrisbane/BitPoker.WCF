using System;
using System.Collections.Generic;
using System.Text;
using PokerEngine;
using PokerEngine.Engine;
using System.Windows;

namespace UltimatePoker
{
    public class GuiMainPlayer : GuiPlayerWrapper
    {
        public GuiMainPlayer(Player player)
            : base(player)
        {
            InitByGameType();
        }

        public GuiMainPlayer(Player player, GuiGames gameType)
            : this(player)
        {
            GameType = gameType;
        }

        private GuiGames gameType = GuiGames.TexasHoldem;

        public GuiGames GameType
        {
            get { return gameType; }
            set
            {
                gameType = value;
                InitByGameType();
            }
        }

        private void InitByGameType()
        {
            if (gameType == GuiGames.TexasHoldem)
            {
                InitSomeActionList(new GuiActions[] { GuiActions.Bet, GuiActions.AllIn }, betActionsList);
                InitSomeActionList(new GuiActions[] { GuiActions.Check, GuiActions.Call, GuiActions.Fold, GuiActions.Deal, GuiActions.Draw, GuiActions.Logout }, roundActionsList);
            }
        }

        private void InitSomeActionList(GuiActions[] guiActions, List<ActionWrapper> toInitList)
        {
            toInitList.Clear();
            foreach (GuiActions action in guiActions)
            {
                ActionWrapper actionWrapper = new ActionWrapper(action);
                toInitList.Add(actionWrapper);
                actionCache[action] = actionWrapper;
            }
        }

        private List<int> raiseAmounts = new List<int>(new int[] { 10, 50, 100, 200, 500, 1000 });
        public List<int> RaiseAmounts
        {
            get { return raiseAmounts; }
        }

        private Dictionary<GuiActions, ActionWrapper> actionCache = new Dictionary<GuiActions, ActionWrapper>();

        private List<ActionWrapper> roundActionsList = new List<ActionWrapper>();
        public IEnumerable<ActionWrapper> RoundActionsList
        {
            get { return roundActionsList; }
        }

        private List<ActionWrapper> betActionsList = new List<ActionWrapper>();
        public IEnumerable<ActionWrapper> BetActionsList
        {
            get { return betActionsList; }
        }

        public void HandleAction(GuiActions action)
        {
            RaiseActionEvent(action);
        }

        public void ChangeActionState(GuiActions action, bool isOn)
        {
            if (actionCache.ContainsKey(action))
            {
                actionCache[action].IsOn = isOn;
            }
        }

        public bool GetActionState(GuiActions action)
        {
            if (actionCache.ContainsKey(action))
                return actionCache[action].IsOn;

            return false;
        }

        private void RaiseActionEvent(GuiActions action)
        {
            if (PlayerPerformedAction != null)
                PlayerPerformedAction(this, new ActionEventArgs(action));
        }

        public event EventHandler<ActionEventArgs> PlayerPerformedAction;

        public event EventHandler<CardDrawEventArgs> PlayerDrawnCard;

        public int CallSum
        {
            get { return (int)GetValue(CallSumProperty); }
            set { SetValue(CallSumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CallSum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CallSumProperty =
            DependencyProperty.Register("CallSum", typeof(int), typeof(GuiMainPlayer), new PropertyMetadata(OnCallSumChanged));



        public int RemainingTime
        {
            get { return (int)GetValue(RemainingTimeProperty); }
            set { SetValue(RemainingTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RemainingTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RemainingTimeProperty =
            DependencyProperty.Register("RemainingTime", typeof(int), typeof(GuiMainPlayer), new UIPropertyMetadata(0));



        public int MinimalBet
        {
            get { return (int)GetValue(MinimalBetProperty); }
            set { SetValue(MinimalBetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinimalBet.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimalBetProperty =
            DependencyProperty.Register("MinimalBet", typeof(int), typeof(GuiMainPlayer), new UIPropertyMetadata(0));


        public override int CurrentBet
        {
            get { return base.CurrentBet; }
            set
            {
                base.CurrentBet = value;
                if (base.CurrentBet < MinimalBet)
                    base.CurrentBet = MinimalBet;
                else if (base.CurrentBet > Money - CallSum)
                    base.CurrentBet = Money - CallSum;
            }
        }

        private static void OnCallSumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GuiMainPlayer)d).actionCache[GuiActions.Call].Tag = e.NewValue;
        }


        internal void HandleCardDraw(CardWrapper card)
        {
            if (PlayerDrawnCard != null)
            {
                PlayerDrawnCard(this, new CardDrawEventArgs(card));
            }
        }
    }

    public class ActionEventArgs : EventArgs
    {
        public ActionEventArgs(GuiActions action)
        {
            this.action = action;
        }

        private GuiActions action;
        public GuiActions Action
        {
            get { return action; }
        }

    }

    public class CardDrawEventArgs : EventArgs
    {
        public CardWrapper Card { get; private set; }
        public CardDrawEventArgs(CardWrapper wrapper)
        {
            Card = wrapper;
        }
    }
}
