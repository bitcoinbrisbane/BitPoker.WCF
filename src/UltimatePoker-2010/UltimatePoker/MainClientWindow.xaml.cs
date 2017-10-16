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
using System.Collections.ObjectModel;using BitPoker.Models.Deck;using UltimatePoker.Controls;
using System.Collections.Generic;
using UltimatePoker.Configuration;

namespace UltimatePoker
{
    public partial class MainClientWindow
    {
        double designedWidth, designedHeight, desginedTop, designedLeft;
        private PokerGameBoard gameBoard = new PokerGameBoard();

        public MainClientWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.Loaded += new RoutedEventHandler(MainClientWindow_Loaded);
            LayoutRoot.DataContext = gameBoard;
            logExapnsion.DataContext = this;
            this.AddHandler(HandDisplay.MouseOnHandEvent, new RoutedEventHandler(HighlightHand));
            this.AddHandler(HandDisplay.ClickOnHandEvent, new RoutedEventHandler(SelectHand));

            desginedTop = Top;
            designedLeft = Left;
            designedHeight = Height;
            designedWidth = Width;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            fixResolution();
        }

        protected override void OnClosed(EventArgs e)
        {
            GuiConfiguration.ExpandLog = LogExpanded;
            GuiConfiguration.GameSpeed = GameSpeed;
            base.OnClosed(e);
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= new EventHandler(SystemEvents_DisplaySettingsChanged);
        }

        void MainClientWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= new RoutedEventHandler(MainClientWindow_Loaded);
            GuiConfiguration configuration = ConfigurationAccess.Current.GuiConfiguration;
            AlwaysHighlight = configuration.StickyHighlighting;
            GameSpeed = configuration.GameSpeed;
            LogExpanded = configuration.ExpandLog;

            if (string.IsNullOrEmpty(configuration.SignInName))
            {
                ChangeName(null, null);
            }
            else
            {
                gameBoard.ThePlayer = new GuiMainPlayer(new Player(configuration.SignInName));
            }

            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);

        }

        void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            fixResolution();
        }

        private static double[][] scaleResolutions = new double[][] { new double[] { 1600, 1200 },
        new double[] {1280, 1024}, new double[] { 1024, 768 }, new double[] { 800, 600 }, new double[] { 640, 480 } };
        private static double[] scaleFactors = new double[] { 1, 0.8, 0.62, 0.46, 0.35 };

        private void fixResolution()
        {
            System.Windows.Forms.Screen mainScreen = System.Windows.Forms.Screen.PrimaryScreen;
            PresentationSource source = PresentationSource.FromVisual(this);

            Matrix tfd = source.CompositionTarget.TransformFromDevice;

            double screenHeight = mainScreen.Bounds.Height;
            double screenWidth = mainScreen.Bounds.Width;
            double scaleFactor = 1;
            double curWidth = 0, curHeight = 0;

            for (int i = 0; i < scaleResolutions.Length; ++i)
            {
                curWidth = scaleResolutions[i][0];
                curHeight = scaleResolutions[i][1];
                scaleFactor = scaleFactors[i];
                if (screenHeight >= curHeight && screenWidth >= curWidth)
                {
                    break;
                }
            }

            // can't suspend layout, I'll just hide for now...
            Hide();

            double scaleX = scaleFactor * tfd.M11;
            double scaleY = scaleFactor * tfd.M22;

            Left = designedLeft * tfd.M11;
            Top = desginedTop * tfd.M22;
            Width = designedWidth * scaleX;
            Height = designedHeight * scaleY;

            layoutTransform.ScaleX = scaleX;
            layoutTransform.ScaleY = scaleY;
            Show();
        }


        public PokerGameBoard GameBoard
        {
            get { return gameBoard; }
        }


        private void SelectHand(object sender, RoutedEventArgs e)
        {
            HandDisplay source = (HandDisplay)e.OriginalSource;
            if (source.PlayerHand != null)
            {
                List<Card> clone = new List<Card>();
                IEnumerator<Card> enumerator = source.PlayerHand.GetAllCards();
                while (enumerator.MoveNext())
                {
                    clone.Add(enumerator.Current);
                }
                gameBoard.SelectExclusiveCards(clone);
                e.Handled = true;
            }
        }
        private void HighlightHand(object sender, RoutedEventArgs e)
        {
            HandDisplay source = (HandDisplay)e.OriginalSource;
            if (source.PlayerHand != null)
            {
                gameBoard.SelectExclusiveCards(new List<Card>(source.PlayerHand));
                e.Handled = true;
            }
        }

        private void UnHighlightHand(object sender, RoutedEventArgs e)
        {
            gameBoard.ClearAllCardsSelection();
        }

        public GuiConfiguration GuiConfiguration { get { return ConfigurationAccess.Current.GuiConfiguration; } }



        public bool AlwaysHighlight
        {
            get { return (bool)GetValue(AlwaysHighlightProperty); }
            set { SetValue(AlwaysHighlightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AlwaysHighlight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AlwaysHighlightProperty =
            DependencyProperty.Register("AlwaysHighlight", typeof(bool), typeof(MainClientWindow), new FrameworkPropertyMetadata(true, OnAlwaysHighlightPropertyChanged));

        private static void OnAlwaysHighlightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainClientWindow t = (MainClientWindow)d;
            t.OnAlwaysHighlightPropertyChanged();

        }

        public bool LogExpanded
        {
            get { return (bool)GetValue(LogExpandedProperty); }
            set { SetValue(LogExpandedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LogExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LogExpandedProperty =
            DependencyProperty.Register("LogExpanded", typeof(bool), typeof(MainClientWindow), new FrameworkPropertyMetadata(true));



        public double GameSpeed
        {
            get { return (double)GetValue(GameSpeedProperty); }
            set { SetValue(GameSpeedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GameSpeed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GameSpeedProperty =
            DependencyProperty.Register("GameSpeed", typeof(double), typeof(MainClientWindow), new FrameworkPropertyMetadata(1.0));



        private void OnAlwaysHighlightPropertyChanged()
        {
            ConfigurationAccess.Current.GuiConfiguration.StickyHighlighting = AlwaysHighlight;
            if (AlwaysHighlight)
            {
                this.RemoveHandler(HandDisplay.MouseOffHandEvent, new RoutedEventHandler(UnHighlightHand));
            }
            else
            {
                if (AutoHandHighlighting)
                    this.AddHandler(HandDisplay.MouseOffHandEvent, new RoutedEventHandler(UnHighlightHand));
            }
        }



        public bool AutoHandHighlighting
        {
            get { return (bool)GetValue(AutoHandHighlightingProperty); }
            set { SetValue(AutoHandHighlightingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoHandHighlighting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoHandHighlightingProperty =
            DependencyProperty.Register("AutoHandHighlighting", typeof(bool), typeof(MainClientWindow),
            new FrameworkPropertyMetadata(true, OnAutoHandHighlightingChanged));

        private static void OnAutoHandHighlightingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MainClientWindow)d).OnAutoHandHighlightingChanged();
        }

        private void OnAutoHandHighlightingChanged()
        {
            if (AutoHandHighlighting)
            {
                this.AddHandler(HandDisplay.MouseOnHandEvent, new RoutedEventHandler(HighlightHand));
                this.AddHandler(HandDisplay.ClickOnHandEvent, new RoutedEventHandler(SelectHand));
                if (!AlwaysHighlight)
                    this.AddHandler(HandDisplay.MouseOffHandEvent, new RoutedEventHandler(UnHighlightHand));
            }
            else
            {
                this.RemoveHandler(HandDisplay.MouseOnHandEvent, new RoutedEventHandler(HighlightHand));
                this.RemoveHandler(HandDisplay.ClickOnHandEvent, new RoutedEventHandler(SelectHand));
            }
        }

        private void TextBlock_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            TextBlock block = (TextBlock)e.Source;
            if (block.Text.Length > 1)
            {
                Storyboard newBoard = new Storyboard();
                newBoard.FillBehavior = FillBehavior.Stop;
                newBoard.Duration = TimeSpan.FromSeconds(1);

                int effectCount = block.TextEffects.Count;
                int textLength = block.Text.Length;


                double letterPercent = 1.0 / (textLength + effectCount);
                for (int i = 0; i < effectCount; ++i)
                {
                    Int32AnimationUsingKeyFrames positionCount = new Int32AnimationUsingKeyFrames();
                    positionCount.BeginTime = TimeSpan.Zero;

                    Storyboard.SetTargetProperty(positionCount, new PropertyPath(string.Format("(TextElement.TextEffects)[{0}].(TextEffect.PositionCount)", i)));

                    if (i > 0)
                    {
                        KeyTime startKeyTime = KeyTime.FromPercent((i - 1) * letterPercent);
                        positionCount.KeyFrames.Add(new DiscreteInt32KeyFrame(0, startKeyTime));
                    }

                    KeyTime moveCountKeyTime = KeyTime.FromPercent(i * letterPercent);
                    positionCount.KeyFrames.Add(new DiscreteInt32KeyFrame(1, moveCountKeyTime));

                    KeyTime endCountKeyTime = KeyTime.FromPercent((textLength + i) * letterPercent);
                    positionCount.KeyFrames.Add(new DiscreteInt32KeyFrame(0, endCountKeyTime));

                    newBoard.Children.Add(positionCount);

                    Int32AnimationUsingKeyFrames positionStart = new Int32AnimationUsingKeyFrames();
                    positionStart.BeginTime = TimeSpan.Zero;

                    Storyboard.SetTargetProperty(positionStart, new PropertyPath(string.Format("(TextElement.TextEffects)[{0}].(TextEffect.PositionStart)", i)));

                    for (int j = 0; j < textLength; ++j)
                    {
                        KeyTime nextStartTime = KeyTime.FromPercent((j + i) * letterPercent);
                        DiscreteInt32KeyFrame nextFrame = new DiscreteInt32KeyFrame(j, nextStartTime);
                        positionStart.KeyFrames.Add(nextFrame);

                    }

                    newBoard.Children.Add(positionStart);
                }

                block.BeginStoryboard(newBoard, HandoffBehavior.SnapshotAndReplace, false);
            }
        }

        private void StartNewGame(object sender, RoutedEventArgs e)
        {
            StartNewGameWizard wizard = new StartNewGameWizard();
            wizard.Owner = this;
            wizard.ShowDialog();
        }

        private void ShowLobby(object sender, RoutedEventArgs e)
        {
            Lobby lobby = new Lobby();
            lobby.Owner = this;
            lobby.ShowDialog();
        }

        private void ChangeName(object sender, RoutedEventArgs e)
        {
            PromptNameWindow getName = new PromptNameWindow();
            getName.Owner = this;
            getName.Title = "Select a new address";
            getName.promptLabel.Content = "Select your new address";

            if (getName.ShowDialog().Value)
            {
                string userName = getName.UserName.Text;
                gameBoard.ThePlayer = new GuiMainPlayer(new Player(userName));
                ConfigurationAccess.Current.GuiConfiguration.SignInName = userName;
            }
        }
    }
}