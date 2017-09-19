using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using UltimatePoker.Configuration;
using PokerEngine.Engine;
using PokerConsole.Engine;
using System.Collections.ObjectModel;
using PokerConsole.AI;
using UltimatePoker.Engine;
using PokerEngine;
using PokerService;
using PokerRules.Games;
using UltimatePoker.AI;

namespace UltimatePoker
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class StartNewGameWizard : Window
    {
        private Thread serverThread;

        private ManualResetEvent waitHandle;

        private DiscoveryHelper helper = new DiscoveryHelper();

        private DispatcherTimer refreshTimer = new DispatcherTimer();


        public StartNewGameWizard()
        {
            InitializeComponent();
            this.DataContext = this;
            GameModes = (ServerGame[])Enum.GetValues(typeof(ServerGame));
            this.Loaded += new RoutedEventHandler(StartNewGameWizard_Loaded);

            refreshTimer.Interval = TimeSpan.FromMilliseconds(500);
            refreshTimer.Tick += new EventHandler(refreshTimer_Tick);

            PreviousAction = NextAction = FinishAction = null;

            enterGameSelectionPage();
            ServerConfigurations = ConfigurationAccess.Current.AllServerConfigurations;
        }

        public IEnumerable<NewServerConfiguration> ServerConfigurations { get; private set; }

        public EventWaitHandle ServerWaitHandle { get { return waitHandle; } }

        public NewServerConfiguration SelectedPreset
        {
            get { return (NewServerConfiguration)GetValue(SelectedPresetProperty); }
            set { SetValue(SelectedPresetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedPreset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedPresetProperty =
            DependencyProperty.Register("SelectedPreset", typeof(NewServerConfiguration), typeof(StartNewGameWizard),
            new UIPropertyMetadata(OnSelectedPresetChanged));

        private static void OnSelectedPresetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((StartNewGameWizard)d).OnSelectedPresetChanged();
        }

        private void OnSelectedPresetChanged()
        {
            TournamentMode = SelectedPreset.TournamentMode;
            Ante = SelectedPreset.Ante;
            SmallRaise = SelectedPreset.SmallRaise;
            StartingMoney = SelectedPreset.StartingMoney;
            Port = SelectedPreset.Port;
            RaiseLimit = SelectedPreset.RaiseLimit;
            AutoRaiseOnHand = SelectedPreset.AutoRaiseOnHand;
            AcceptsNewPlayers = SelectedPreset.AcceptsNewPlayers;
            TimeLimit = SelectedPreset.PlayerTimeLimit;
        }

        private Action prevAction, nextAction, finishAction;

        private void doNothing() { }

        private Action PreviousAction
        {
            get { return prevAction; }
            set
            {
                if (value == null)
                {
                    prevAction = doNothing;
                    prev.IsEnabled = false;
                }
                else
                {
                    prev.IsEnabled = true;
                    prevAction = value;
                }
            }
        }

        private Action NextAction
        {
            get { return nextAction; }
            set
            {
                if (value == null)
                {
                    nextAction = doNothing;
                    next.IsEnabled = false;
                }
                else
                {
                    next.IsEnabled = true;
                    nextAction = value;
                }
            }
        }

        private Action FinishAction
        {
            get { return finishAction; }
            set
            {
                if (value == null)
                {
                    finishAction = doNothing;
                    finish.IsEnabled = false;
                }
                else
                {
                    finish.IsEnabled = true;
                    finishAction = value;
                }
            }
        }

        private void enterGameSelectionPage()
        {
            PreviousAction = null;
            LayoutRoot.SelectedItem = selectGameTypeTab;
            NextAction = exitGameSelectionPage;
        }

        void exitGameSelectionPage()
        {
            PreviousAction = enterGameSelectionPage;

            if (singlePlayer.IsChecked.Value)
            {
                enterCreateSingleplayerServerPage();
            }
            else if (create.IsChecked.Value)
            {
                enterCreateMultiplayerServerPage();
            }
            else
                enterJoinGamePage();

        }

        private void enterCreateSingleplayerServerPage()
        {
            LayoutRoot.SelectedItem = createNewGameTab;
            PreviousAction = enterGameSelectionPage;
            NextAction = null;
            FinishAction = exitCreateSingleplayerServerPage;
        }

        private void exitCreateSingleplayerServerPage()
        {
            if (BotCount < 1)
                BotCount = 1;
            next.IsEnabled = false;
            prev.IsEnabled = false;
            finish.IsEnabled = false;
            connectionView.Visibility = Visibility.Visible;
            createServer();
        }

        private void enterJoinGamePage()
        {
            NextAction = null;
            FinishAction = exitJoinGamePage;
            LayoutRoot.SelectedItem = joinGameTab;
            PreviousAction = clearFinish(PreviousAction);
            refreshTimer_Tick(null, null);
        }

        private Action clearFinish(Action realAction)
        {
            return delegate
            {
                FinishAction = null;
                realAction();
            };
        }

        private void exitJoinGamePage()
        {
            connectionView.Visibility = Visibility.Visible;
            Connect();
        }

        void enterCreateMultiplayerServerPage()
        {
            LayoutRoot.SelectedItem = createNewGameTab;
            NextAction = exitCreateMultiplayerServerPage;
            PreviousAction = enterGameSelectionPage;
            BotCount = 0;
        }

        private void exitCreateMultiplayerServerPage()
        {
            PreviousAction = enterCreateMultiplayerServerPage;
            enterWaitForPlayersPage();
            createServer();
        }

        private void enterWaitForPlayersPage()
        {
            LayoutRoot.SelectedItem = waitForUsersTab;
            NextAction = null;
            FinishAction = exitWaitForPlayersPage;
            PreviousAction = null;
            finish.IsEnabled = false;
            connectionView.Visibility = Visibility.Visible;
            // finish will be enabled after the server is initialized
        }

        private void exitWaitForPlayersPage()
        {
            waitHandle.Set();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (waitHandle != null)
                waitHandle.Set();
            refreshTimer.Stop();

            GameMode lastMode = GameMode.SinglePlayer;
            if (multiplayer.IsChecked.Value)
            {
                if (join.IsChecked.Value)
                    lastMode = GameMode.Multipayer;
                else if (create.IsChecked.Value)
                    lastMode = GameMode.MultiplayerHost;
                else
                    lastMode = GameMode.Spectator;
            }
            ConfigurationAccess.Current.GuiConfiguration.LastGameMode = lastMode;
        }
        private int playerLimit = int.MaxValue;

        private void createServer()
        {
            if (serverThread == null)
            {
                CreateServerThreadArguments args = new CreateServerThreadArguments();

                CanCreate = false;

                BaseEngine server = null;
                WcfEngineHost binaryHelper = new WcfEngineHost();
                waitHandle = new ManualResetEvent(false);
                switch (SelectedGame)
                {
                    case ServerGame.FiveCardDraw: server = new FiveGameDrawServer(binaryHelper); break;
                    case ServerGame.TexasHoldem: server = new TexasHoldemServer(binaryHelper); break;
                    case ServerGame.SevenCardStud: server = new SevenCardStudServer(binaryHelper); break;
                    case ServerGame.OmahaHoldem: server = new OmahaHoldemServer(binaryHelper); break;
                }
                binaryHelper.Initialize(server, SelectedGame, Port, waitHandle);
                if (multiplayer.IsChecked.Value)
                {
                    server.GameStarted += new DataEventHandler<IEnumerable<Player>>(server_GameStarted);
                }
                playerLimit = server.MaximalPlayersLimit;

                args.game = server;

                NewServerConfiguration config = ConfigurationAccess.Current.NewServerConfiguration;
                config.TournamentMode = server.TournamentMode = TournamentMode;
                config.AcceptsNewPlayers = server.AcceptPlayersAfterGameStart = AcceptsNewPlayers;
                config.Ante = server.Ante = Ante;
                config.SmallRaise = server.SmallRaise = SmallRaise;
                config.RaiseLimit = server.RaiseLimit = RaiseLimit;
                config.StartingMoney = server.StartingMoney = StartingMoney;
                config.PlayerTimeLimit = server.ActionTimeout = TimeLimit;
                config.AutoRaiseOnHand = server.AutoIncreaseOnHandDivider = AutoRaiseOnHand;
                config.SelectedGame = args.gameMode = SelectedGame;
                config.BotCount = args.botCount = BotCount;
                config.Port = args.port = Port;

                ServerAddress = "localhost";
                ServerPort = Port;

                waitHandle.Reset();

                serverThread = new Thread(new ParameterizedThreadStart(StartServer));
                serverThread.Start(args);
                serverThread.Name = "Game Server Thread";
                serverThread.IsBackground = true;

                if (multiplayer.IsChecked.Value)
                {
                    binaryHelper.PlayerConnected += new DataEventHandler<Player>(client_NewUserConnected);
                }
            }
        }

        private void StartServer(object threadArguments)
        {
            try
            {
                CreateServerThreadArguments args = (CreateServerThreadArguments)threadArguments;
                BaseEngine server = args.game;
                server.Initialize();
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(serverInitialized));
                if (args.botCount > 0)
                {
                    spawBots(args.botCount, args.gameMode, args.port);
                }
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(serverIsUp));
                server.Run();
            }
            catch
            {

            }
            finally
            {
                serverThread = null;
            }
        }

        private void serverInitialized()
        {
            if (multiplayer.IsChecked.Value)
            {
                setTimer = new DispatcherTimer();
                setTimer.Interval = TimeSpan.FromSeconds(1);
                setTimer.Tick += new EventHandler(
                    delegate
                    {
                        setTimer.Stop();
                        connectionView.Visibility = Visibility.Hidden;
                        finish.IsEnabled = true;
                    });
                setTimer.Start();
            }
        }

        private void serverIsUp()
        {
            Connect();
        }

        private void spawBots(int botCount, ServerGame serverGame, int port)
        {
            try
            {
                for (int i = 0; i < botCount; ++i)
                {
                    BaseWcfClient curBot = null;
                    RulesInterpreterBridge rulesBridge = new RulesInterpreterBridge();
                    switch (serverGame)
                    {
                        case ServerGame.FiveCardDraw: curBot = new BaseWcfClient(new FiveGameDrawClient(new AiFiveCardDrawClient(new EmptyClientHelper(), rulesBridge))); break;
                        case ServerGame.OmahaHoldem: curBot = new BaseWcfClient(new OmahaHoldemClient(new AiClientHelper(new EmptyClientHelper(), rulesBridge))); break;
                        case ServerGame.SevenCardStud: curBot = new BaseWcfClient(new GameClient<SevenCardStudGame>(new AiClientHelper(new EmptyClientHelper(), rulesBridge))); break;
                        case ServerGame.TexasHoldem: curBot = new BaseWcfClient(new GameClient<TexasHoldem>(new AiClientHelper(new EmptyClientHelper(), rulesBridge))); break;
                    }
                    rulesBridge.Interpreter = (IRulesInterpreter)curBot.ConcreteClient;
                    Thread tempThread = new Thread(
                        delegate()
                        {
                            try
                            {
                                curBot.Initialize("localhost", port);
                                curBot.Connect();
                                curBot.Run();
                                curBot.Disconnect();
                            }
                            catch
                            {
                            }
                        });
                    tempThread.IsBackground = true;
                    tempThread.Priority = ThreadPriority.BelowNormal;
                    tempThread.Name = string.Format("AI thread {0}/{1}", port, i);
                    tempThread.Start();
                }
                Thread.Sleep(TimeSpan.FromSeconds(1 * botCount));
            }
            catch
            {
            }
        }

        void server_GameStarted(object sender, DataEventArgs<IEnumerable<Player>> e)
        {
            BaseEngine server = (BaseEngine)sender;
            server.GameStarted -= new DataEventHandler<IEnumerable<Player>>(server_GameStarted);
            waitHandle.Reset();
            // the wait handle was passed to the gui helper, don't set it anymore
            waitHandle = null;
        }

        internal void Connect()
        {
            //prev.IsEnabled = false;
            //finish.IsEnabled = false;

            CreateClientThreadArguments args = new CreateClientThreadArguments();
            ClientHelperBridge bridge = new ClientHelperBridge();
            RulesInterpreterBridge rulesBridge = new RulesInterpreterBridge();

            bridge.ClientHelper = new GameClient<TexasHoldem>(new GuiHelper(new TexasHoldemGuiClient(rulesBridge, 2)));
            rulesBridge.Interpreter = (IRulesInterpreter)bridge.ClientHelper;

            args.address = ServerAddress;
            args.port = ServerPort;
            args.client = bridge;
            args.rulesBridge = rulesBridge;
            args.game = ServerGame.TexasHoldem;
            args.spectate = spectate.IsChecked.Value && multiplayer.IsChecked.Value;
            args.isSinglePlayer = singlePlayer.IsChecked.Value;

            ThreadPool.QueueUserWorkItem(StartGame, args);
        }

        private void StartGame(object state)
        {
            try
            {
                CreateClientThreadArguments args = (CreateClientThreadArguments)state;
                ClientHelperBridge client = args.client;

                BaseWcfClient wcfClient = new BaseWcfClient(client);
                ServerDetails result = wcfClient.Initialize(args.address, args.port);

                if (result.CanConnect || args.spectate)
                {
                    if (result.Game != args.game)
                    {
                        args.game = result.Game;

                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action<CreateClientThreadArguments>(UpdateClient), args);
                    }
                    GuiHelper guiHelper = ((ClientHelperDecorator)wcfClient.ConcreteClient).FindWrappedHelper<GuiHelper>();
                    // don't pass the server wait handle when the player plays alone
                    if (args.isSinglePlayer)
                        guiHelper.Initialize(wcfClient, !args.spectate, null);
                    else
                        guiHelper.Initialize(wcfClient, !args.spectate, waitHandle);
                    
                    if (args.spectate)
                    {
                        if (!wcfClient.RequestSpectation())
                            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(CantSpectate));
                    }
                    else
                        wcfClient.Connect();

                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action<CreateClientThreadArguments>(DidConnect), args);
                    //wcfClient.Run();
                    //wcfClient.Disconnect();
                }
                else
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(GameInProgress));

            }
            catch (Exception e)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action<Exception>(NotifyError), e);
            }
        }

        private void UpdateClient(CreateClientThreadArguments args)
        {
            switch (args.game)
            {
                case ServerGame.FiveCardDraw: args.client.FiveCardHelper = new FiveGameDrawClient(new FiveCardGuiHelper(new FiveCardDrawGuiClient(args.rulesBridge))); break;
                case ServerGame.OmahaHoldem: args.client.ClientHelper = new OmahaHoldemClient(new GuiHelper(new TexasHoldemGuiClient(args.rulesBridge, 4))); break;
                case ServerGame.SevenCardStud: args.client.ClientHelper = new GameClient<SevenCardStudGame>(new GuiHelper(new ConcreteHelper(args.rulesBridge))); break;
            }
            args.rulesBridge.Interpreter = (IRulesInterpreter)args.client.ClientHelper;
            args.client.NotifyConnectedToServer(null);
            args.client.NotifyRunningGame(args.game, 0);
        }

        private void CantSpectate()
        {
            generalError("Server refused spectation.", "Can't spectate", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void GameInProgress()
        {
            generalError("Game is already in progress", "Can't connect", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NotifyError(Exception e)
        {
            generalError(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void generalError(string message, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            if (IsVisible)
            {
                connectionView.Visibility = Visibility.Hidden;
                MessageBox.Show(message, caption, button, image);
                prev.IsEnabled = true;
                finish.IsEnabled = true;
            }
        }

        private DispatcherTimer setTimer;
        private void DidConnect(CreateClientThreadArguments threadArgs)
        {
            ConfigurationAccess.Current.ServerConfiguration.Port = ServerPort;
            ConfigurationAccess.Current.ServerConfiguration.Address = ServerAddress;


            if (singlePlayer.IsChecked.Value || (!create.IsChecked.Value))
            {
                setTimer = new DispatcherTimer();
                setTimer.Interval = TimeSpan.FromSeconds(1);
                setTimer.Tick += new EventHandler(setTimer_Tick);
                setTimer.Start();
            }
        }

        void client_NewUserConnected(object sender, DataEventArgs<Player> e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action<string>(newPlayerConnected), e.Data.Name);
        }

        private void newPlayerConnected(string playerName)
        {
            connectedPlayers.Items.Add(playerName);
            if (connectedPlayers.Items.Count >= playerLimit)
                exitWaitForPlayersPage();
        }

        void setTimer_Tick(object sender, EventArgs e)
        {
            Close();
            setTimer.Stop();
            if (waitHandle != null)
                waitHandle.Set();
        }

        public ServiceLocation SelectedServer
        {
            get { return (ServiceLocation)GetValue(SelectedServerProperty); }
            set { SetValue(SelectedServerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedServer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedServerProperty =
            DependencyProperty.Register("SelectedServer", typeof(ServiceLocation), typeof(StartNewGameWizard),
            new FrameworkPropertyMetadata(OnSelectedServerChanged));

        private static void OnSelectedServerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((StartNewGameWizard)d).OnSelectedServerChanged();
        }

        private void OnSelectedServerChanged()
        {

            if (SelectedServer != null)
            {
                Uri fullAddress = SelectedServer.Endpoint.Uri;
                ServerAddress = fullAddress.DnsSafeHost;
                ServerPort = fullAddress.Port;
            }
        }

        void StartNewGameWizard_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= new RoutedEventHandler(StartNewGameWizard_Loaded);


            SelectedPreset = ConfigurationAccess.Current.NewServerConfiguration;

            Port = SelectedPreset.Port;
            SelectedGame = SelectedPreset.SelectedGame;
            BotCount = SelectedPreset.BotCount;

            ServerAddress = ConfigurationAccess.Current.ServerConfiguration.Address;
            ServerPort = ConfigurationAccess.Current.ServerConfiguration.Port;

            switch (ConfigurationAccess.Current.GuiConfiguration.LastGameMode)
            {
                case GameMode.SinglePlayer: singlePlayer.IsChecked = true; break;
                case GameMode.Multipayer: multiplayer.IsChecked = true; join.IsChecked = true; break;
                case GameMode.MultiplayerHost: multiplayer.IsChecked = true; create.IsChecked = true; break;
                case GameMode.Spectator: multiplayer.IsChecked = true; spectate.IsChecked = true; break;
            }

            refreshTimer.Start();
        }

        public string ServerAddress
        {
            get { return (string)GetValue(ServerAddressProperty); }
            set { SetValue(ServerAddressProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Address.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ServerAddressProperty =
            DependencyProperty.Register("ServerAddress", typeof(string), typeof(StartNewGameWizard), new FrameworkPropertyMetadata(string.Empty));


        public int ServerPort
        {
            get { return (int)GetValue(ServerPortProperty); }
            set { SetValue(ServerPortProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Port.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ServerPortProperty =
            DependencyProperty.Register("ServerPort", typeof(int), typeof(StartNewGameWizard), new FrameworkPropertyMetadata(0));

        private ObservableCollection<ServiceLocation> discoveredServers = new ObservableCollection<ServiceLocation>();

        public ObservableCollection<ServiceLocation> DiscoveredServers
        {
            get { return discoveredServers; }
        }

        public IEnumerable<ServerGame> GameModes
        {
            get { return (IEnumerable<ServerGame>)GetValue(GameModesProperty); }
            set { SetValue(GameModesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GameModes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GameModesProperty =
            DependencyProperty.Register("GameModes", typeof(IEnumerable<ServerGame>), typeof(StartNewGameWizard));




        public ServerGame SelectedGame
        {
            get { return (ServerGame)GetValue(SelectedGameProperty); }
            set { SetValue(SelectedGameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedGame.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedGameProperty =
            DependencyProperty.Register("SelectedGame", typeof(ServerGame), typeof(StartNewGameWizard));


        public bool AcceptsNewPlayers
        {
            get { return (bool)GetValue(AcceptsNewPlayersProperty); }
            set { SetValue(AcceptsNewPlayersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AcceptsNewPlayers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AcceptsNewPlayersProperty =
            DependencyProperty.Register("AcceptsNewPlayers", typeof(bool), typeof(StartNewGameWizard));



        public bool TournamentMode
        {
            get { return (bool)GetValue(TournamentModeProperty); }
            set { SetValue(TournamentModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TournamentMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TournamentModeProperty =
            DependencyProperty.Register("TournamentMode", typeof(bool), typeof(StartNewGameWizard));



        public bool CanCreate
        {
            get { return (bool)GetValue(CanCreateProperty); }
            set { SetValue(CanCreateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanCreate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanCreateProperty =
            DependencyProperty.Register("CanCreate", typeof(bool), typeof(StartNewGameWizard), new FrameworkPropertyMetadata(true));



        public int Port
        {
            get { return (int)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Port.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register("Port", typeof(int), typeof(StartNewGameWizard), new UIPropertyMetadata(6060));



        public int BotCount
        {
            get { return (int)GetValue(BotCountProperty); }
            set { SetValue(BotCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BotCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BotCountProperty =
            DependencyProperty.Register("BotCount", typeof(int), typeof(StartNewGameWizard), new UIPropertyMetadata(0));



        public int AutoRaiseOnHand
        {
            get { return (int)GetValue(AutoRaiseOnHandProperty); }
            set { SetValue(AutoRaiseOnHandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoRaiseOnHand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoRaiseOnHandProperty =
            DependencyProperty.Register("AutoRaiseOnHand", typeof(int), typeof(StartNewGameWizard), new UIPropertyMetadata(7));



        public int SmallRaise
        {
            get { return (int)GetValue(SmallRaiseProperty); }
            set { SetValue(SmallRaiseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SmallRaise.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SmallRaiseProperty =
            DependencyProperty.Register("SmallRaise", typeof(int), typeof(StartNewGameWizard), new UIPropertyMetadata(250));



        public int Ante
        {
            get { return (int)GetValue(AnteProperty); }
            set { SetValue(AnteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Ante.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AnteProperty =
            DependencyProperty.Register("Ante", typeof(int), typeof(StartNewGameWizard), new UIPropertyMetadata(100));




        public int RaiseLimit
        {
            get { return (int)GetValue(RaiseLimitProperty); }
            set { SetValue(RaiseLimitProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RaiseLimit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RaiseLimitProperty =
            DependencyProperty.Register("RaiseLimit", typeof(int), typeof(StartNewGameWizard), new UIPropertyMetadata(3));




        public int StartingMoney
        {
            get { return (int)GetValue(StartingMoneyProperty); }
            set { SetValue(StartingMoneyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartingMoney.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartingMoneyProperty =
            DependencyProperty.Register("StartingMoney", typeof(int), typeof(StartNewGameWizard), new UIPropertyMetadata(5000));



        public int TimeLimit
        {
            get { return (int)GetValue(TimeLimitProperty); }
            set { SetValue(TimeLimitProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimeLimit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeLimitProperty =
            DependencyProperty.Register("TimeLimit", typeof(int), typeof(StartNewGameWizard), new UIPropertyMetadata(0));


        private class CreateServerThreadArguments
        {
            public BaseEngine game;

            public ServerGame gameMode;

            public int botCount;

            public int port;
        }


        private class CreateClientThreadArguments
        {
            public bool isSinglePlayer;

            public string address;

            public int port;

            public ClientHelperBridge client;

            public ServerGame game;

            public RulesInterpreterBridge rulesBridge;

            public bool spectate;
        }

        private void prev_Click(object sender, RoutedEventArgs e)
        {
            PreviousAction();
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            NextAction();
        }

        private void finish_Click(object sender, RoutedEventArgs e)
        {
            FinishAction();
        }

        void refreshTimer_Tick(object sender, EventArgs e)
        {
            Func<IEnumerable<ServiceLocation>> call = new Func<IEnumerable<ServiceLocation>>(helper.Discover);
            call.BeginInvoke(OnDiscovered, call);
            refreshTimer.IsEnabled = false;
        }

        private void OnDiscovered(IAsyncResult result)
        {
            try
            {
                if (CheckAccess())
                {
                    Func<IEnumerable<ServiceLocation>> call = (Func<IEnumerable<ServiceLocation>>)result.AsyncState;
                    IEnumerable<ServiceLocation> discovered = call.EndInvoke(result);

                    int prevCount = discoveredServers.Count;
                    discoveredServers.Clear();

                    foreach (ServiceLocation msg in discovered)
                    {
                        discoveredServers.Add(msg);
                    }

                    if (prevCount != discoveredServers.Count || discoveredServers.Count == 0)
                    {
                        refreshTimer.Interval = TimeSpan.FromMilliseconds(500);
                    }
                    else if (refreshTimer.Interval.TotalMilliseconds < 30000)
                    {
                        refreshTimer.Interval = refreshTimer.Interval + refreshTimer.Interval;
                    }
                    // don't restart after the window has closed
                    refreshTimer.IsEnabled = this.IsVisible;
                }
                else
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new AsyncCallback(OnDiscovered), result);
                }
            }
            catch
            {
                refreshTimer.Stop();
            }
        }
    }
}
