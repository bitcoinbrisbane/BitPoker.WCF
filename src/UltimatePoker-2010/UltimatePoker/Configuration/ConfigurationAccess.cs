using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Configuration;

namespace UltimatePoker.Configuration
{
    /// <summary>
    /// A helper class which loads and saves the application configuration.
    /// </summary>
    public class ConfigurationAccess
    {
        // the configuration loaded.
        private System.Configuration.Configuration configuration;
        // the single instance of the configuration
        private static ConfigurationAccess instance = new ConfigurationAccess();

        /// <summary>
        /// 	<para>Initializes an instance of the <see cref="ConfigurationAccess"/> class.</para>
        /// </summary>
        /// <remarks>
        /// The constructor is private since this class is a singleton
        /// </remarks>
        private ConfigurationAccess()
        {
            // monitor the application exit. Try save when the program finishes
            Application.Current.Exit += new ExitEventHandler(Current_Exit);
            // open the application configuration file
            configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        /// <summary>
        /// called when the application exits.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// This method tries to save the configuration file when the application is closing
        /// </remarks>
        void Current_Exit(object sender, ExitEventArgs e)
        {
            try
            {

                configuration.Save();
            }
            catch (ConfigurationErrorsException ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error saving configuration. ({0})", ex.Message));
            }
        }

        /// <summary>
        /// Gets the single instance of the configuration
        /// </summary>
        public static ConfigurationAccess Current
        {
            get { return instance; }
        }

        /// <summary>
        /// Gets the <see cref="ServerConfiguration"/> section in the configuration file
        /// </summary>
        public ServerConfiguration ServerConfiguration { get { return (ServerConfiguration)configuration.GetSection("serverConfiguration"); } }

        /// <summary>
        /// Gets the <see cref="GuiConfiguration"/> section in the configuration file
        /// </summary>
        public GuiConfiguration GuiConfiguration { get { return (GuiConfiguration)configuration.GetSection("guiConfiguration"); } }

        /// <summary>
        /// Gets the <see cref="NewServerConfiguration"/> section in the configuration file
        /// </summary>
        public NewServerConfiguration NewServerConfiguration { get { return (NewServerConfiguration)configuration.GetSection("newServerConfiguration"); } }

        /// <summary>
        /// Gets all predefined server configurations
        /// </summary>
        public ICollection<NewServerConfiguration> AllServerConfigurations
        {
            get
            {
                List<NewServerConfiguration> configurations = new List<NewServerConfiguration>();
                foreach (ConfigurationSection section in configuration.Sections)
                    if (section is NewServerConfiguration)
                        configurations.Add((NewServerConfiguration)section);
                
                configurations.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));

                return configurations;
            }
        }
    }
}
