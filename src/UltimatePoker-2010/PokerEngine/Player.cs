using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using BitPoker.Models.Deck;

namespace PokerEngine
{
    /// <summary>
    /// A class representing a player.
    /// </summary>
    /// <remarks>
    /// The player is identified using it's name. It holds the player money, current bet and cards.
    /// It implements <see cref="INotifyPropertyChanged"/> to allow GUI interaction.
    /// </remarks>
    [Serializable]
    public class Player : INotifyPropertyChanged
    {
        //public BitPoker.Crypto.IWallet Wallet { get; private set; }

        /// <summary>
        /// Creates a new instance of the Player class.
        /// </summary>
        /// <param name="address">The player name</param>
        public Player(string address)
        {
            //this.Wallet = new BitPoker.Crypto.Bitcoin(key);
            this.Name = address;
        }

        //private string name;
        /// <summary>
        /// Gets the player name.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }


        private int money;
        /// <summary>
        /// Gets or sets the player money.
        /// </summary>
        public int Money
        {
            get
            {
                return (money);
            }
            set
            {
                if (money != value)
                {
                    money = value;
                    OnPropertyChanged("Money");
                }
            }
        }


        private int curBet;
        /// <summary>
        /// Gets or sets the player current bet.
        /// </summary>
        public int CurrentBet
        {
            get
            {
                return (curBet);
            }
            set
            {
                if (curBet != value)
                {   
                    curBet = value;
                    OnPropertyChanged("CurrentBet");
                }
            }
        }


        private ObservableCollection<Card> cards = new ObservableCollection<Card>();

        /// <summary>
        /// An observable collection of cards which holds the current player cards.
        /// </summary>
        public ObservableCollection<Card> Cards
        {
            get { return cards; }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event with the given property name.
        /// </summary>
        /// <param name="propName">The property name which was modified</param>
        protected void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes. <see cref="INotifyPropertyChanged"/>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// Returns the player name and money in a string format.
        /// </summary>
        /// <returns>
        /// A String that represents the current player.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} ({1} BTC)", Name, money);
        }
    }
}
