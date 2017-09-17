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
    /// Interaction logic for Wallet.xaml
    /// </summary>
    public partial class Wallet : Window
    {
        private readonly ViewModels.WalletViewModel _viewModel;

        public Wallet()
        {
            _viewModel = new ViewModels.WalletViewModel();
            this.DataContext = _viewModel;

            InitializeComponent();
        }
    }
}
