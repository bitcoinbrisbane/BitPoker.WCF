using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for Lobby.xaml
    /// </summary>
    public partial class Lobby : Window
    {
        private readonly ViewModels.LobbyViewModel _viewModel;

        public Lobby()
        {
            _viewModel = new ViewModels.LobbyViewModel();
            this.DataContext = _viewModel;

            InitializeComponent();
        }
    }
}
