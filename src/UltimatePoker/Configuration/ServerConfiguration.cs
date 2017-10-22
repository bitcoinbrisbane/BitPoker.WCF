using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace UltimatePoker.Configuration
{
    /// <summary>
    /// A configuration section which stores the connected server configuration.
    /// </summary>
    public class ServerConfiguration : ConfigurationSection
    {
        // the server address
        private static readonly ConfigurationProperty addressProperty = new ConfigurationProperty("address", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        // the server port
        private static readonly ConfigurationProperty portProperty = new ConfigurationProperty("port", typeof(int), 0, ConfigurationPropertyOptions.IsRequired);
        // the section properties
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        /// <summary>
        /// Initializes the section properties
        /// </summary>
        static ServerConfiguration()
        {
            properties.Add(addressProperty);
            properties.Add(portProperty);
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
        /// Gets or sets the server address.
        /// </summary>
        /// <seealso cref="UltimatePoker.MainWindow.Address"/>
        public string Address
        {
            get { return (string)base[addressProperty]; }
            set { base[addressProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the server port.
        /// </summary>
        /// <seealso cref="UltimatePoker.MainWindow.Port"/>
        public int Port
        {
            get { return (int)base[portProperty]; }
            set { base[portProperty] = value; }
        }
    }
}
