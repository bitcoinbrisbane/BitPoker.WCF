using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using PokerRules.Deck;
using System.Windows.Data;

namespace UltimatePoker.Controls
{
	/// <summary>
	/// a Control that draws the suite of the card according to the content, which is</br>
	/// assumed to be a <see cref="PokerRules.Deck.Card"/>.
	/// </summary>
	public class SuiteControl : ContentControl
	{

		/// <summary>Raises the <see cref="E:System.Windows.FrameworkElement.Initialized"/> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized"/> is set to true internally. </summary>
		/// <returns/>
		/// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs"/> that contains the event data.</param>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			// Inherit the content from containing control
			BindingOperations.SetBinding(this, ContentControl.ContentProperty, new Binding());
		}

		/// <summary>
		/// Chooses a ControlTemplate of a suite accorind to the card.
		/// </summary>
		/// <param name="oldContent"></param>
		/// <param name="newContent"></param>
		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged(oldContent, newContent);

			Card currentCard = Card.Empty;

			if (newContent is Card)
				currentCard = (Card)newContent;
			else
				return;

			if (currentCard.CardSuite == Suite.Clobes)
				Template = TryFindResource("Clubs") as ControlTemplate;
			else if (currentCard.CardSuite == Suite.Diamonds)
				Template = TryFindResource("Diamonds") as ControlTemplate;
			else if (currentCard.CardSuite == Suite.Hearts)
				Template = TryFindResource("Hearts") as ControlTemplate;
			else if (currentCard.CardSuite == Suite.Spades)
				Template = TryFindResource("Spades") as ControlTemplate;
		}
	}
}
