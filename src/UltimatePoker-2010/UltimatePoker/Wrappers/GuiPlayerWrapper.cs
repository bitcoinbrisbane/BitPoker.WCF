using System;
using System.Collections.Generic;
using System.Text;
using PokerEngine;
using System.Windows;
using System.Collections.ObjectModel;
using PokerRules.Deck;
using System.Collections.Specialized;
using System.Collections;
using PokerRules.Hands;
using System.Windows.Media;

namespace UltimatePoker
{
    public class GuiPlayerWrapper : DependencyObject
    {

        public GuiPlayerWrapper(Player player)
        {
            ThePlayer = player;
            InitCards();
        }

        #region Cards Init

        private void InitCards()
        {
            AddCards(ThePlayer.Cards);
            ThePlayer.Cards.CollectionChanged += new NotifyCollectionChangedEventHandler(PlayerCardsChanged);
        }

        void PlayerCardsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                AddCards(e.NewItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                RemoveCards(e.OldItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.cards.Clear();
            }
        }

        private void AddCards(IEnumerable cards)
        {
            foreach (Card card in cards)
            {
                this.cards.Add(new CardWrapper(card));
            }
        }

        private void RemoveCards(IEnumerable cards)
        {
            CardWrapper foundCard = null;

            foreach (Card card in cards)
            {
                foreach (CardWrapper cardWrapper in this.cards)
                {
                    if (cardWrapper.Card == card)
                    {
                        foundCard = cardWrapper;
                        break;
                    }
                }
                if (foundCard != null)
                {
                    this.cards.Remove(foundCard);
                }
            }
        }

        #endregion

        #region ThePlayer Property

        private const string ThePlayerPropertyName = "ThePlayer";

        /// <summary>
        /// ThePlayer DependencyProperty of GuiPlayerWrapper.
        /// </summary>
        public static readonly DependencyProperty ThePlayerProperty =
          DependencyProperty.Register(ThePlayerPropertyName, typeof(Player), typeof(GuiPlayerWrapper),
                                      new UIPropertyMetadata(null));

        /// <summary>
        /// ThePlayer Property of GuiPlayerWrapper.
        /// </summary>
        public Player ThePlayer
        {
            get { return ((Player)GetValue(ThePlayerProperty)); }
            set { SetValue(ThePlayerProperty, value); }
        }

        #endregion

        #region Cards Property

        private ObservableCollection<CardWrapper> cards = new ObservableCollection<CardWrapper>();
        public ObservableCollection<CardWrapper> Cards
        {
            get { return cards; }
        }

        #endregion

        #region IsActive Property

        private const string IsActivePropertyName = "IsActive";

        /// <summary>
        /// IsActive DependencyProperty of GuiPlayerWrapper.
        /// </summary>
        public static readonly DependencyProperty IsActiveProperty =
          DependencyProperty.Register(IsActivePropertyName, typeof(bool), typeof(GuiPlayerWrapper),
                                      new UIPropertyMetadata(false));

        /// <summary>
        /// IsActive Property of GuiPlayerWrapper.
        /// </summary>
        public bool IsActive
        {
            get { return ((bool)GetValue(IsActiveProperty)); }
            set { SetValue(IsActiveProperty, value); }
        }

        #endregion

        #region Message Property

        private const string MessagePropertyName = "Message";

        /// <summary>
        /// Message DependencyProperty of GuiPlayerWrapper.
        /// </summary>
        public static readonly DependencyProperty MessageProperty =
          DependencyProperty.Register(MessagePropertyName, typeof(string), typeof(GuiPlayerWrapper),
                                      new UIPropertyMetadata(null));

        /// <summary>
        /// Message Property of GuiPlayerWrapper.
        /// </summary>
        public string Message
        {
            get { return ((string)GetValue(MessageProperty)); }
            set { SetValue(MessageProperty, value); }
        }

        #endregion



        public SolidColorBrush AssociatedColor
        {
            get { return (SolidColorBrush)GetValue(AssociatedColorProperty); }
            set { SetValue(AssociatedColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AssociatedColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AssociatedColorProperty =
            DependencyProperty.Register("AssociatedColor", typeof(SolidColorBrush), typeof(GuiPlayerWrapper), new UIPropertyMetadata(Brushes.White));



        public Hand CurrentHand
        {
            get { return (Hand)GetValue(CurrentHandProperty); }
            set { SetValue(CurrentHandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentHand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentHandProperty =
            DependencyProperty.Register("CurrentHand", typeof(Hand), typeof(GuiPlayerWrapper));



        public bool IsDealer
        {
            get { return (bool)GetValue(IsDealerProperty); }
            set { SetValue(IsDealerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDealer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDealerProperty =
            DependencyProperty.Register("IsDealer", typeof(bool), typeof(GuiPlayerWrapper), new UIPropertyMetadata(false));



        public int PotInvestment
        {
            get { return (int)GetValue(PotInvestmentProperty); }
            set { SetValue(PotInvestmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PotInvestment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PotInvestmentProperty =
            DependencyProperty.Register("PotInvestment", typeof(int), typeof(GuiPlayerWrapper), new UIPropertyMetadata(0));



        #region Wrap Properties

        public string Name
        {
            get { return ThePlayer.Name; }
        }

        public int Money
        {
            get { return ThePlayer.Money; }
            set { ThePlayer.Money = value; }
        }

        public virtual int CurrentBet
        {
            get { return ThePlayer.CurrentBet; }
            set { ThePlayer.CurrentBet = value; }
        }

        #endregion

        private void ClearCardSelections()
        {
            foreach (CardWrapper card in this.cards)
            {
                card.IsSelected = false;
            }
        }

        public void SelectCard(Card card)
        {
            SelectCard(new CardWrapper(card));
        }

        public void SelectCard(CardWrapper card)
        {
            foreach (CardWrapper cardWrapper in Cards)
            {
                if (card.Card == cardWrapper.Card)
                {
                    cardWrapper.IsSelected = true;
                }
            }
        }

        public void SelectHand(IEnumerable<CardWrapper> hand)
        {
            ClearCardSelections();
            foreach (CardWrapper card in hand)
            {
                SelectCard(card);
            }
        }

        public void SelectHand(IEnumerable<Card> hand)
        {
            ClearCardSelections();
            foreach (Card card in hand)
            {
                SelectCard(card);
            }
        }
    }
}
