using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltimatePoker.ViewModels
{
    public class PotViewModel : PokerEngine.Betting.Pot, INotifyPropertyChanged
    {
        private int curRaise;

        ///// <summary>
        ///// Gets the current raise sum. The current raise can be raised by any player using a call to <see cref="Raise"/>
        ///// </summary>
        //public override int CurrentRaise
        //{
        //    get
        //    {
        //        return (curRaise);
        //    }
        //    private set
        //    {
        //        if (curRaise != value)
        //        {
        //            curRaise = value;
        //            OnPropertyChanged("CurrentRaise");
        //        }
        //    }
        //}

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes. <see cref="INotifyPropertyChanged"/>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
