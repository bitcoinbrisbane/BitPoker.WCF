using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PokerRules.Hands;

namespace UltimatePoker.Controls
{
    /// <summary>
    /// A simple text block which has a <see cref="Hand"/> attached to it.
    /// </summary>
    public class HandDisplay : TextBlock
    {
        static HandDisplay()
        {   
            //Define a new style key for the control
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HandDisplay), new FrameworkPropertyMetadata(typeof(HandDisplay)));
        }

        /// <summary>
        /// An event which is raised on a <see cref="HandDisplay"/> when the mouse is over the hand.
        /// </summary>
        public static RoutedEvent MouseOnHandEvent = EventManager.RegisterRoutedEvent("MouseOnHand", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HandDisplay));

        /// <summary>
        /// An event which is raised on a <see cref="HandDisplay"/> when the mouse is off the hand
        /// </summary>
        public static RoutedEvent MouseOffHandEvent = EventManager.RegisterRoutedEvent("MouseOffHand", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HandDisplay));

        /// <summary>
        /// An event which is raised on a <see cref="HandDisplay"/> when a mouse left button is clicked.
        /// </summary>
        public static RoutedEvent ClickOnHandEvent = EventManager.RegisterRoutedEvent("ClickOnHand", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HandDisplay));


        /// <summary>
        /// Gets or sets the attached player hand.
        /// </summary>
        public Hand PlayerHand
        {
            get { return (Hand)GetValue(PlayerHandProperty); }
            set { SetValue(PlayerHandProperty, value); }
        }

        /// <summary>
        /// The dependency property behind the <see cref="PlayerHand"/> property.
        /// </summary>
        public static readonly DependencyProperty PlayerHandProperty =
            DependencyProperty.Register("PlayerHand", typeof(Hand), typeof(HandDisplay));

        /// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseEnter"/> attached event is raised on this element. Implement this method to add class handling for this event. </summary>
        /// <returns/>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        /// <remarks>
        /// The <see cref="MouseOnHandEvent"/> is raised here
        /// </remarks>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(MouseOnHandEvent, this));
            base.OnMouseEnter(e);
        }

        /// <summary>Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseLeave"/> attached event is raised on this element. Implement this method to add class handling for this event. </summary>
        /// <returns/>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        /// <remarks>The <see cref="MouseOffHandEvent"/> is raised here
        /// </remarks>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(MouseOffHandEvent, this));
            base.OnMouseLeave(e);
        }

        /// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.MouseLeftButtonDown"/> routed event is raised on this element. Implement this method to add class handling for this event. </summary>
        /// <returns/>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that the left mouse button was pressed.</param>
        /// <remarks>
        /// The <see cref="ClickOnHandEvent"/> event is raised here.
        /// </remarks>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // capture the mouse so if the button is raised somewhere else it will be known
            CaptureMouse();
            RaiseEvent(new RoutedEventArgs(ClickOnHandEvent, this));
            base.OnMouseLeftButtonDown(e);
        }

        /// <summary>Invoked when an unhandled <see cref="E:System.Windows.UIElement.MouseLeftButtonUp"/> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. </summary>
        /// <returns/>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that the left mouse button was released.</param>
        /// <remarks>
        /// The <see cref="MouseOnHandEvent"/> is raised here.
        /// </remarks>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(MouseOnHandEvent, this));
            base.OnMouseLeftButtonUp(e);
            // release the mouse capture which was captrues in the call to OnMouseLeftButtonDown
            ReleaseMouseCapture();
        }
    }
}
