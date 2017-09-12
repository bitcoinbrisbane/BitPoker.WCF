using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows;

namespace UltimatePoker
{

    public class InvertedBooleanToVisibilityConverter : IValueConverter
    {
        private BooleanToVisibilityConverter converter = new BooleanToVisibilityConverter();


        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility result = (Visibility)converter.Convert(value, targetType, parameter, culture);
            if (result == Visibility.Visible)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = (bool)converter.ConvertBack(value, targetType, parameter, culture);
            return !result;
        }

        #endregion
    }
}
