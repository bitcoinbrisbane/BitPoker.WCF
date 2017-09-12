using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using PokerRules.Deck;

namespace UltimatePoker
{
    /// <summary>
    /// A converter which converts card values to human readable strings
    /// </summary>
	public class CardValueConverter : IValueConverter
	{
		#region IValueConverter Members

        /// <summary>Converts a value. </summary>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				return Binding.DoNothing;
            
			int cardValue = (int)value;
            // check the card is in the valid range
			if (cardValue > -1 && cardValue < Card.CARDS_IN_SUITE)
			{
				string name = string.Empty;
                // recall that the cards are sorted by their game value.
				if (cardValue < 9 && cardValue > -1)
					name = "" + (cardValue + 2);
				else
				{
					switch (cardValue)
					{
						case 9: name = "J"; break;
						case 10: name = "Q"; break;
						case 11: name = "K"; break;
						case 12: name = "A"; break;
					}
				}

				return name;
			}
            
			return Binding.DoNothing;
		}

        /// <summary>Converts a value. </summary>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}

		#endregion
	}
}
