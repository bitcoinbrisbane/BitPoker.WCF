using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace UltimatePoker.Configuration
{
    /// <summary>
    /// A configuration section which stores the related GUI configuration
    /// </summary>
    public class GuiConfiguration : ConfigurationSection
    {
        // the sticky highlighting propery
        private static readonly ConfigurationProperty stickyHighlighting = new ConfigurationProperty("stickyHighlighting", typeof(bool), true);
        // the sign in name propertu
        private static readonly ConfigurationProperty signInNameProperty = new ConfigurationProperty("signInName", typeof(string), string.Empty);
        // the game mode property
        private static readonly ConfigurationProperty gameModeProperty = new ConfigurationProperty("gameMode", typeof(GameMode), GameMode.SinglePlayer);
        // the game speed property
        private static readonly ConfigurationProperty gameSpeedProperty = new ConfigurationProperty("gameSpeed", typeof(double), 1.0);
        // the default expansion of the log
        private static readonly ConfigurationProperty expandLogProperty = new ConfigurationProperty("expandLog", typeof(bool), true);

        // the section properties
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        /// <summary>
        /// Initializes the section properties
        /// </summary>
        static GuiConfiguration()
        {
            properties.Add(stickyHighlighting);
            properties.Add(signInNameProperty);
            properties.Add(gameModeProperty);
            properties.Add(gameSpeedProperty);
            properties.Add(expandLogProperty);
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
        /// Gets or sets the sticky highlighting property. Default is true. 
        /// </summary>
        /// <seealso cref="UltimatePoker.MainClientWindow.AlwaysHighlight"/>
        public bool StickyHighlighting
        {
            get { return (bool)base[stickyHighlighting]; }
            set { base[stickyHighlighting] = value; }
        }

        /// <summary>
        /// Gets or sets the user sign in name property. Default is <see cref="string.Empty"/>
        /// </summary>
        /// <seealso cref="UltimatePoker.MainWindow.UserName"/>
        public string SignInName
        {
            get { return (string)base[signInNameProperty]; }
            set { base[signInNameProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the last game mode. Default is <see cref="GameMode.SinglePlayer"/>
        /// </summary>
        public GameMode LastGameMode
        {
            get { return (GameMode)base[gameModeProperty]; }
            set { base[gameModeProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the expand log property. Default is true. 
        /// </summary>
        /// <seealso cref="UltimatePoker.MainClientWindow.LogExpanded"/>
        public bool ExpandLog
        {
            get { return (bool)base[expandLogProperty]; }
            set { base[expandLogProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the game speed property. Default is 1.0
        /// </summary>
        /// <seealso cref="UltimatePoker.MainClientWindow.GameSpeed"/>
        public double GameSpeed
        {
            get { return (double)base[gameSpeedProperty]; }
            set { base[gameSpeedProperty] = value; }
        }
    }

    /// <summary>
    /// An enum which defines the last mode the player played.
    /// </summary>
    public enum GameMode
    {
        /// <summary>
        /// Single player mode
        /// </summary>
        SinglePlayer,
        /// <summary>
        /// Multiplayer mode which hosted the game
        /// </summary>
        MultiplayerHost,
        /// <summary>
        /// Multiplayer mode which didn't host the game
        /// </summary>
        Multipayer,
        Spectator,
    }
}
