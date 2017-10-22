using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace UltimatePoker.Converters
{
    /// <summary>
    /// A converter which is used to convert the speed to a text value
    /// </summary>
    /// <remarks>
    /// The speed slowest rate should be passed as an argument
    /// </remarks>
    public class SpeedTextConverter : IValueConverter
    {
        private static string[] speedRates = new string[] { "Warp 10", "Fastest", "Faster", "Normal", "Slower", "Slowest", "Turtle Face" };
        #region IValueConverter Members

        /// <summary>Converts a value. </summary>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // get the value & the slowest rate
            double curValue = (double)value;
            double slowestRate = (double)parameter;
            double speedInterval = slowestRate / speedRates.Length;
            // search the speed interval
            for (int i = 1; i < speedRates.Length; ++i)
                if (curValue < i * speedInterval)
                    // return the string value
                    return speedRates[i - 1];
            // never reached
            return speedRates[speedRates.Length - 1];

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
