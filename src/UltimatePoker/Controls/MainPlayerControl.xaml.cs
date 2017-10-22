using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using PokerEngine;
using System.Collections.Generic;
using System.Windows.Input;

namespace UltimatePoker
{
    /// <summary>
    /// The control which manages the current player interaction with the game
    /// </summary>
    public partial class MainPlayerControl
    {
        private SolidColorBrush lowBrush, mediumBrush;
        private const double LOW_THRESHOLD = 0.1;
        private const double MEDIUM_THRESHOLD = 0.25;
        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="MainPlayerControl"/> class.</para>
        /// </summary>
        public MainPlayerControl()
        {
            this.InitializeComponent();
            timeLimit.ValueChanged += new RoutedPropertyChangedEventHandler<double>(timeLimit_ValueChanged);

            lowBrush = (SolidColorBrush)FindResource("lowBrush");
            mediumBrush = (SolidColorBrush)FindResource("mediumBrush");
            lowBrush.Freeze();
            mediumBrush.Freeze();
        }

        void timeLimit_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (timeLimit.Value < LOW_THRESHOLD * timeLimit.Maximum)
            {
                timeLimit.Foreground = lowBrush;
            }
            else if (timeLimit.Value < MEDIUM_THRESHOLD * timeLimit.Maximum)
            {
                timeLimit.Foreground = mediumBrush;
            }
            else
                timeLimit.ClearValue(ProgressBar.ForegroundProperty);

        }


        /// <summary>
        /// A callback for the player left mouse button click on a bet button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BetUpButtonClicked(object sender, RoutedEventArgs args)
        {
            // change the bet up
            changePlayerBet(sender, 1);

        }

        /// <summary>
        /// Changes the <see cref="GuiMainPlayer.CurrentBet"/> according to the button clicked
        /// </summary>
        /// <param name="sender">The sending button which was clicked</param>
        /// <param name="betMultiplier">A multiplier in which the bet value is multiplied. 1 to increase, -1 to decrease</param>
        private void changePlayerBet(object sender, int betMultiplier)
        {
            // must have a valid player as the data context to change the current bet value
            GuiMainPlayer thePlayer = DataContext as GuiMainPlayer;
            if (thePlayer == null)
                return;

            // the bet value is also the button tag
            Button clickedButton = (Button)sender;
            int betValue = (int)clickedButton.Tag;
            // change the current bet by the given value
            thePlayer.CurrentBet += (betValue * betMultiplier);
        }

        /// <summary>
        /// A callback for the player right mouse button click on a bet button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BetDownButtonClicked(object sender, MouseButtonEventArgs args)
        {
            // change the bet down
            changePlayerBet(sender, -1);
        }

        /// <summary>
        /// A callback for a player action button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ActionButtonClicked(object sender, RoutedEventArgs args)
        {
            // must have a valid player as the data context to handle the current action
            GuiMainPlayer thePlayer = DataContext as GuiMainPlayer;
            if (thePlayer == null)
                return;
            // the action is also the button tag
            Button clickedButton = (Button)sender;
            GuiActions action = (GuiActions)clickedButton.Tag;
            // send the player the action played
            thePlayer.HandleAction(action);
        }

        /// <summary>
        /// A callback for a player card click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CardButtonClicked(object sender, RoutedEventArgs e)
        {
            // must have a valid player as the data context to handle the current draw action
            GuiMainPlayer thePlayer = DataContext as GuiMainPlayer;
            if (thePlayer == null)
                return;
            // the card is also the button data context
            Button clickedCard = (Button)e.OriginalSource;
            CardWrapper wrapper = clickedCard.DataContext as CardWrapper;
            // send the player the button which was clicked.
            if (wrapper != null)
                thePlayer.HandleCardDraw(wrapper);
        }
    }
}