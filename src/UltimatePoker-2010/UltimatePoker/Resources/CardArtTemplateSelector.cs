using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using BitPoker.Models.Deck;
using System.Windows;

namespace UltimatePoker
{
    /// <summary>
    /// Selects the correct card art 
    /// </summary>
    public class CardArtTemplateSelector : DataTemplateSelector
    {
        private static string[] resourcesKeys = new string[] 
        {"TwoCardTemplate","ThreeCardTemplate","FourCardTemplate","FiveCardTemplate",
            "SixCardTemplate","SevenCardTemplate","EightCardTemplate","NineCardTemplate","TenCardTemplate","JackCardTemplate",
            "QueenCardTemplate","KingCardTemplate","AceCardTemplate"};
        /// <summary>When overridden in a derived class, returns a <see cref="T:System.Windows.DataTemplate"/> based on custom logic.</summary>
        /// <returns>Returns a <see cref="T:System.Windows.DataTemplate"/> or null. The default value is null.</returns>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            if (item == null)
                base.SelectTemplate(item, container);

            Card currentCard = (Card)item;
            FrameworkElement element = (FrameworkElement)container;

            string key = resourcesKeys[currentCard.CardValue];
        
            return element.TryFindResource(key) as DataTemplate;

        }
    }
}
