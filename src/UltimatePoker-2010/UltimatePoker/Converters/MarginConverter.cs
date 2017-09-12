using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace UltimatePoker
{
    /// <summary>
    /// A converter which is used to lift a selected card higher than the other cards.
    /// </summary>
    /// <remarks>
    /// The card is lifted by changing the margin
    /// </remarks>
    [ValueConversion(typeof(bool),typeof(Thickness))]
    public class MarginConverter : IValueConverter
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
            bool realValue = (bool)value;
            if (realValue)
            {
                // lift the card up
                return new Thickness(0, -10, 0, 0);
            }
            return DependencyProperty.UnsetValue;
        }

        /// <summary>Converts a value. </summary>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}
