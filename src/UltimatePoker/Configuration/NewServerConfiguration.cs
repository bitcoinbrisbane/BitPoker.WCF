using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using PokerService;

namespace UltimatePoker.Configuration
{
    /// <summary>
    /// A configuration section which stores the data related to a new server setup.
    /// </summary>
    public class NewServerConfiguration : ConfigurationSection
    {
        // the configuration display name
        private static readonly ConfigurationProperty displayNameProperty = new ConfigurationProperty("displayName", typeof(string), null);
        // the server port property
        private static readonly ConfigurationProperty portProperty = new ConfigurationProperty("port", typeof(int), 6060);
        // the selected game type property
        private static readonly ConfigurationProperty selectedGameProperty = new ConfigurationProperty("selectedGame", typeof(ServerGame), ServerGame.FiveCardDraw);
        // the tournament mode property
        private static readonly ConfigurationProperty tournamentModeProperty = new ConfigurationProperty("tournamentMode", typeof(bool), true);
        // the ante property
        private static readonly ConfigurationProperty anteProperty = new ConfigurationProperty("ante", typeof(int), 100);
        // the small raise property
        private static readonly ConfigurationProperty smallRaiseProperty = new ConfigurationProperty("smallRaise", typeof(int), 250);
        // the raise limit property
        private static readonly ConfigurationProperty raiseLimitProperty = new ConfigurationProperty("raiseLimit", typeof(int), 3);
        // the starting money property
        private static readonly ConfigurationProperty startingMoneyProperty = new ConfigurationProperty("startingMoney", typeof(int), 5000);
        // the bot count property
        private static readonly ConfigurationProperty botCountProperty = new ConfigurationProperty("botCount", typeof(int), 0);
        // the auto raise on hand divider property
        private static readonly ConfigurationProperty autoRaiseOnHandProperty = new ConfigurationProperty("autoRaiseOnHand", typeof(int), 7);
        // the accepts new players property
        private static readonly ConfigurationProperty acceptsNewPlayersProperty = new ConfigurationProperty("acceptsNewPlayers", typeof(bool), false);
        // the time limit property
        private static readonly ConfigurationProperty playerTimeLimitProperty = new ConfigurationProperty("playerTimeLimit", typeof(int), 0);
        // the section properties
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        /// <summary>
        /// Initializes the section properties
        /// </summary>
        static NewServerConfiguration()
        {
            properties.Add(displayNameProperty);
            properties.Add(portProperty);
            properties.Add(selectedGameProperty);
            properties.Add(tournamentModeProperty);
            properties.Add(anteProperty);
            properties.Add(smallRaiseProperty);
            properties.Add(raiseLimitProperty);
            properties.Add(startingMoneyProperty);
            properties.Add(botCountProperty);
            properties.Add(autoRaiseOnHandProperty);
            properties.Add(acceptsNewPlayersProperty);
            properties.Add(playerTimeLimitProperty);
        }

        /// <summary>Gets the collection of properties.</summary>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationPropertyCollection"/> of properties for the element.</returns>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }

        /// <summary>
        /// Gets or sets the new server port. Default is 6060
        /// </summary>
        /// <seealso cref="UltimatePoker.CreateServer.Port"/>
        public int Port
        {
            get { return (int)base[portProperty]; }
            set { base[portProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the new server ante. Default is 100.
        /// </summary>
        /// <seealso cref="PokerEngine.Engine.BaseEngine.Ante"/>
        public int Ante
        {
            get { return (int)base[anteProperty]; }
            set { base[anteProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the new server small raise amount. Default is 250.
        /// </summary>
        /// <seealso cref="PokerEngine.Engine.BaseEngine.SmallRaise"/>
        public int SmallRaise
        {
            get { return (int)base[smallRaiseProperty]; }
            set { base[smallRaiseProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the server raise limit count. Default is 3
        /// </summary>
        /// <seealso cref="PokerEngine.Engine.BaseEngine.RaiseLimit"/>
        public int RaiseLimit
        {
            get { return (int)base[raiseLimitProperty]; }
            set { base[raiseLimitProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the server starting money. Default is 5000.
        /// </summary>
        /// <seealso cref="PokerEngine.Engine.BaseEngine.StartingMoney"/>
        public int StartingMoney
        {
            get { return (int)base[startingMoneyProperty]; }
            set { base[startingMoneyProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the new server game type. Default is <see cref="ServerGame.FiveCardDraw"/>
        /// </summary>
        /// <seealso cref="UltimatePoker.CreateServer.SelectedGame"/>
        public ServerGame SelectedGame
        {
            get { return (ServerGame)base[selectedGameProperty]; }
            set { base[selectedGameProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the new server tournament mode. Default is true.
        /// </summary>
        /// <seealso cref="PokerEngine.Engine.BaseEngine.TournamentMode"/>
        public bool TournamentMode
        {
            get { return (bool)base[tournamentModeProperty]; }
            set { base[tournamentModeProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the accept new players mode. Default is false.
        /// </summary>
        /// <seealso cref="PokerEngine.Engine.BaseEngine.AcceptPlayersAfterGameStart"/>
        public bool AcceptsNewPlayers
        {
            get { return (bool)base[acceptsNewPlayersProperty]; }
            set { base[acceptsNewPlayersProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the new server default bot count. Default is 0.
        /// </summary>
        /// <seealso cref="<seealso cref="UltimatePoker.CreateServer.BotCount"/>
        public int BotCount
        {
            get { return (int)base[botCountProperty]; }
            set { base[botCountProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the new server raise hand divider. The default is 7.
        /// </summary>
        /// <seealso cref="PokerEngine.Engine.BaseEngine.AutoIncreaseOnHandDivider"/>
        public int AutoRaiseOnHand
        {
            get { return (int)base[autoRaiseOnHandProperty]; }
            set { base[autoRaiseOnHandProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the server time limit for player actions. The default is 0.
        /// </summary>
        /// <seealso cref="PokerEngine.Engine.BaseEngine.ActionTimeout"/>
        public int PlayerTimeLimit
        {
            get { return (int)base[playerTimeLimitProperty]; }
            set { base[playerTimeLimitProperty] = value; }
        }

        /// <summary>
        /// Gets the section display name
        /// </summary>
        public string DisplayName
        {
            get { return (string)base[displayNameProperty]; }
        }
    }
}
