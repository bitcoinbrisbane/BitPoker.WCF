using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace UltimatePoker
{
    [ValueConversion(typeof(int), typeof(Visibility))]
    public class PositiveIntToVisibilityConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility positiveResult = Visibility.Visible;
            Visibility negativeResult = Visibility.Collapsed;
            if (parameter != null)
            {
                positiveResult = Visibility.Collapsed;
                negativeResult = Visibility.Visible;
            }

            if (value is int)
            {
                int realValue = (int)value;
                if (realValue > 0)
                    return positiveResult;
                else
                    return negativeResult;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}
