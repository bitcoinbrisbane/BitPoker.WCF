using System;
using System.Collections.Generic;
using System.Text;
using PokerEngine.Engine;
using PokerConsole.Engine;
using PokerConsole.AI;
using PokerService;
using PokerRules.Games;

namespace PokerConsole
{
    /// <summary>
    /// The entry point class
    /// </summary>
    class Program
    {
        /// <summary>
        /// The console application entry class
        /// </summary>
        /// <param name="args">
        /// The command line arguments.
        /// </param>
        static void Main(string[] args)
        {
            try
            {
                int port = -1;
                string serverIp = string.Empty;
                ServerGame game = ServerGame.TexasHoldem;
                BitPoker.Crypto.IWallet wallet;

                // print the command line usage if the arguments do not match
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: <server address | Game Mode> <port>");
                    Console.WriteLine("server address: ip or dns");
                    Console.WriteLine("Game Mode: texas,five,seven,omaha");
                    Console.WriteLine("port: the port in which the server runs");

                    //Console.WriteLine("Enter option: ");
                    //serverIp = Console.ReadLine();
                    serverIp = "";
                    port = 5000;
                    game = ServerGame.TexasHoldem;
                }
                else
                {
                    port = int.Parse(args[1]);
                    // test to see if the game mode is recognized
                    if (args[0] == "texas")
                    {
                        game = ServerGame.TexasHoldem;
                    }
                    //else if (args[0] == "five")
                    //{
                    //    game = ServerGame.FiveCardDraw;
                    //}
                    //else if (args[0] == "seven")
                    //{
                    //    game = ServerGame.SevenCardStud;
                    //}
                    //else if (args[0] == "omaha")
                    //{
                    //    game = ServerGame.OmahaHoldem;
                    //}
                    else // assume the argument is an ip or dns
                    {
                        serverIp = args[0];
                    }
                }

                // found a match to the game
                if (serverIp == string.Empty)
                {
                    BaseEngine server = null;
                    WcfEngineHost binaryHelper = new WcfEngineHost();
                    switch (game)
                    {
                        case ServerGame.TexasHoldem: server = new TexasHoldemServer(binaryHelper); break;
                        //case ServerGame.FiveCardDraw: server = new FiveGameDrawServer(binaryHelper); break;
                        //case ServerGame.SevenCardStud: server = new SevenCardStudServer(binaryHelper); break;
                        //case ServerGame.OmahaHoldem: server = new OmahaHoldemServer(binaryHelper); break;
                    }

                    System.Threading.ManualResetEvent waitHandle = new System.Threading.ManualResetEvent(false);
                    binaryHelper.Initialize(server, game, port, waitHandle);

                    // initialize the server and run until the game is over
                    server.Initialize();
                    Console.WriteLine("Press enter to stop the registration");
                    Console.ReadLine();
                    waitHandle.Set();
                    server.Run();
                    waitHandle.Close();
                }
                else
                {
                    // if the arguments count is higher than 2 assume the process started will be played by an AI engine.
                    bool useAi = args.Length > 3;

                    //TODO: REGEX
                    string wifKey = args[2];
                    wallet = new BitPoker.Crypto.Bitcoin(wifKey.Trim(), true);

                    // this is the base client which will hold the connected game                        
                    ClientHelperBridge clientBridge = new ClientHelperBridge(wallet);

                    RulesInterpreterBridge rulesBridge = new RulesInterpreterBridge();
                    BaseWcfClient client = new BaseWcfClient(clientBridge);
                    clientBridge.ClientHelper = new ConsoleClientHelper(rulesBridge);
                    ServerDetails result = client.Initialize(serverIp, port);

                    // check the result of the connection
                    if (result.CanConnect)
                    {
                        IClientHelper outerClient = null;
                        if (useAi)
                        {
                            switch (result.Game)
                            {
                                case ServerGame.FiveCardDraw: outerClient = new FiveGameDrawClient(new AiFiveCardDrawClient(new ConsoleFiveGameDrawClient(rulesBridge), rulesBridge)); break;
                                case ServerGame.OmahaHoldem: outerClient = new OmahaHoldemClient(new AiClientHelper(new ConsoleTexasHoldemClient(rulesBridge, 4), rulesBridge)); break;
                                case ServerGame.SevenCardStud: outerClient = new GameClient<SevenCardStudGame>(new AiClientHelper(new ConsoleClientHelper(rulesBridge), rulesBridge)); break;
                                case ServerGame.TexasHoldem: outerClient = new GameClient<TexasHoldem>(new AiClientHelper(new ConsoleTexasHoldemClient(rulesBridge), rulesBridge)); break;
                            }
                        }
                        else
                        {
                            switch (result.Game)
                            {
                                case ServerGame.FiveCardDraw: outerClient = new FiveGameDrawClient(new ConsoleFiveGameDrawClient(rulesBridge)); break;
                                case ServerGame.OmahaHoldem: outerClient = new OmahaHoldemClient(new ConsoleTexasHoldemClient(rulesBridge, 4)); break;
                                case ServerGame.SevenCardStud: outerClient = new GameClient<SevenCardStudGame>(new ConsoleClientHelper(rulesBridge)); break;
                                case ServerGame.TexasHoldem: outerClient = new GameClient<TexasHoldem>(new ConsoleTexasHoldemClient(rulesBridge)); break;
                            }
                        }

                        IRulesInterpreter interpreter = (IRulesInterpreter)outerClient;
                        rulesBridge.Interpreter = interpreter;
                            
                        if (result.Game == ServerGame.FiveCardDraw)
                            clientBridge.FiveCardHelper = (IFiveCardClientHelper)outerClient;
                        else
                            clientBridge.ClientHelper = outerClient;

                        client.Connect();
                        // run until the game is over
                        client.Run();
                        client.Disconnect();
                    }
                    else
                        Console.WriteLine("Game is in progress. Server refused connection");
                }

            }
            catch (Exception e)
            {
                // an error occured. print an unfriendly message
                Console.WriteLine("Uh oh, I did bad");
                // when a debugger is attached, break it so you can learn of the error.
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Console.WriteLine(e.Message);
                    System.Diagnostics.Debugger.Break();
                }
            }
        }
    }
}
