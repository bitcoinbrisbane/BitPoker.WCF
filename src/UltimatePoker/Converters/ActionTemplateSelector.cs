using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace UltimatePoker.Converters
{
    /// <summary>
    /// A template selector which enables specific action buttons templates
    /// </summary>
    class ActionTemplateSelector : DataTemplateSelector
    {
        /// <summary>When overridden in a derived class, returns a <see cref="T:System.Windows.DataTemplate"/> based on custom logic.</summary>
        /// <returns>Returns a <see cref="T:System.Windows.DataTemplate"/> or null. The default value is null.</returns>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ActionWrapper wrapper = (ActionWrapper)item;
            FrameworkElement element = (FrameworkElement)container;
            // when the action is a Call use a different data template which will pase the call amount on the button
            if (wrapper.Action == GuiActions.Call)
                return (DataTemplate)element.TryFindResource("CallButtonTemplate");
            // in all other cases use the default template
            return (DataTemplate)element.TryFindResource("ActionButtonTemplate");
        }
    }
}
