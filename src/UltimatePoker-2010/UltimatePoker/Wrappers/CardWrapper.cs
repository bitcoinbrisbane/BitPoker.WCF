using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using BitPoker.Models.Deck;

namespace UltimatePoker
{
	public class CardWrapper : INotifyPropertyChanged
	{
		public CardWrapper(Card card)
		{
			this.card = card;
		}

		private Card card;
		public Card Card
		{
			get { return card; }
		}

		private bool isSelected;

		public bool IsSelected
		{
			get { return isSelected; }
			set 
			{ 
				isSelected = value;
				RaiseChangedEvent("IsSelected");
			}
		}

		private void RaiseChangedEvent(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}


		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
