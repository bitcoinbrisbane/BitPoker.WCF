using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UltimatePoker
{
    /// <summary>
    /// Interaction logic for PromptNameWindow.xaml
    /// </summary>

    public partial class PromptNameWindow : System.Windows.Window
    {

        public PromptNameWindow()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            this.UserName.Focus();
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }
    }
}