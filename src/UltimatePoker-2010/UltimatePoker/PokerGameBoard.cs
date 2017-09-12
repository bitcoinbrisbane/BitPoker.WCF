using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using PokerEngine;
using System.Collections.ObjectModel;
using PokerRules.Deck;

namespace UltimatePoker
{
    public class PokerGameBoard : DependencyObject
    {
        public PokerGameBoard()
        {
            ThePlayer = new GuiMainPlayer(new Player("Unnamed Player"));
        }

        #region ThePlayer Property

        private const string ThePlayerPropertyName = "ThePlayer";


        /// <summary>
        /// ThePlayer DependencyProperty of PokerGameBoard.
        /// </summary>
        public static readonly DependencyProperty ThePlayerProperty =
          DependencyProperty.Register(ThePlayerPropertyName, typeof(GuiMainPlayer), typeof(PokerGameBoard),
                                      new UIPropertyMetadata(null));

        /// <summary>
        /// ThePlayer Property of PokerGameBoard.
        /// </summary>
        public GuiMainPlayer ThePlayer
        {
            get { return ((GuiMainPlayer)GetValue(ThePlayerProperty)); }
            set { SetValue(ThePlayerProperty, value); }
        }

        #endregion

        #region OtherPlayers Property

        private ObservableCollection<GuiPlayerWrapper> otherPlayers = new ObservableCollection<GuiPlayerWrapper>();

        public ObservableCollection<GuiPlayerWrapper> OtherPlayers
        {
            get { return otherPlayers; }
            set { otherPlayers = value; }
        }

        #endregion

        #region Pot Property

        private const string PotPropertyName = "Pot";

        /// <summary>
        /// Pot DependencyProperty of PokerGameBoard.
        /// </summary>
        public static readonly DependencyProperty PotProperty =
          DependencyProperty.Register(PotPropertyName, typeof(int), typeof(PokerGameBoard),
                                      new UIPropertyMetadata(0));

        /// <summary>
        /// Pot Property of PokerGameBoard.
        /// </summary>
        public int Pot
        {
            get { return ((int)GetValue(PotProperty)); }
            set { SetValue(PotProperty, value); }
        }

        #endregion

        #region CommunityCards Property

        private ObservableCollection<CardWrapper> communityCards = new ObservableCollection<CardWrapper>();

        public ObservableCollection<CardWrapper> CommunityCards
        {
            get { return communityCards; }
        }

        #endregion

        private GuiPlayerWrapper activePlayer = null;
        public GuiPlayerWrapper ActivePlayer
        {
            get { return activePlayer; }
        }





        public bool IsRunning
        {
            get { return (bool)GetValue(IsRunningProperty); }
            set { SetValue(IsRunningProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRunning.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRunningProperty =
            DependencyProperty.Register("IsRunning", typeof(bool), typeof(PokerGameBoard), new UIPropertyMetadata(false));



        public string BoardTitle
        {
            get { return (string)GetValue(BoardTitleProperty); }
            set { SetValue(BoardTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BoardTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BoardTitleProperty =
            DependencyProperty.Register("BoardTitle", typeof(string), typeof(PokerGameBoard), new UIPropertyMetadata("Ultimate Poker"));




        public void ActivatePlayer(GuiPlayerWrapper player)
        {
            if (activePlayer != null)
                activePlayer.IsActive = false;

            activePlayer = player;
            activePlayer.IsActive = true;
        }

        public void PostExclusiveMessage(GuiPlayerWrapper player, string message)
        {
            ClearAllMessages();
            player.Message = message;
        }

        public void ClearAllMessages()
        {
            ThePlayer.Message = string.Empty;
            foreach (GuiPlayerWrapper player in OtherPlayers)
            {
                player.Message = string.Empty;
            }
        }

        public void ClearAllHands()
        {
            ThePlayer.CurrentHand = null;
            foreach (GuiPlayerWrapper wrapper in OtherPlayers)
            {
                wrapper.CurrentHand = null;
            }
        }

        private void SetAllCardsSelection(Predicate<Card> predicate)
        {
            foreach (CardWrapper wrapper in CommunityCards)
            {
                wrapper.IsSelected = predicate(wrapper.Card);
            }
            foreach (GuiPlayerWrapper player in OtherPlayers)
            {
                foreach (CardWrapper wrapper in player.Cards)
                    wrapper.IsSelected = predicate(wrapper.Card);
            }

            foreach (CardWrapper wrapper in ThePlayer.Cards)
                wrapper.IsSelected = predicate(wrapper.Card);
        }

        public void ClearAllCardsSelection()
        {
            SetAllCardsSelection(
                delegate(Card ignored)
                {
                    return false;
                });
        }

        public void SelectExclusiveCards(IList<Card> cards)
        {
            SetAllCardsSelection(
                delegate(Card cur)
                {
                    return cards.Contains(cur);
                });
        }
    }
}
