using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UltimatePoker
{
    /// <summary>
    /// A converter which converts bet values to colors.
    /// </summary>
	public class ChipColorConverter : IValueConverter
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

			int chipValue = (int)value;
            
			switch (chipValue)
			{
				case 10:
					return "#FFFFD700";
				case 50:
					return "#FFFF6800";
				case 100:
					return "#FFF20F0F";
				case 200:
					return "#FFB404FF";
				case 500:
					return "#FF00AD44";
				case 1000:
					return "#FF0016AD";
				default:
					break;
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
			throw new NotImplementedException();
		}

		#endregion
	}
}
