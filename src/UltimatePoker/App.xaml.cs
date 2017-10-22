using System;
using System.Windows;
using System.Data;
using System.Xml;
using System.Configuration;
using System.Threading;
using System.ServiceModel;

namespace UltimatePoker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : System.Windows.Application
    {

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // TODO - try to preload wcf services so it won't be so slow
        }
    }
}