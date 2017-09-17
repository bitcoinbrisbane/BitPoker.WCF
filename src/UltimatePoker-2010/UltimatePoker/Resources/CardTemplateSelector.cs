using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using BitPoker.Models.Deck;
using System.Windows;

namespace UltimatePoker
{
	public class CardTemplateSelector : DataTemplateSelector
	{
		public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
		{
			if (item == null)
				return null;

			CardWrapper currentCard = (CardWrapper)item;
            FrameworkElement element = (FrameworkElement)container;

			if (currentCard.Card == Card.Empty)
                return element.TryFindResource("CardBackTemplate") as DataTemplate;
			else
                return element.TryFindResource("CardFrontTemplate") as DataTemplate;
		}
	}
}
