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
using System.Windows.Navigation;
using System.Windows.Shapes;
using PokerEngine;

namespace UltimatePoker
{
    /// <summary>
    /// Interaction logic for PotInformation.xaml
    /// </summary>
    public partial class PotInformation : UserControl
    {
        public PotInformation()
        {
            InitializeComponent();
        }

        public void UpdatePotInformation(IEnumerable<Player> playerOrder, int[][] newPotData)
        {

            int playerCount = newPotData.Length;
            int potsCount = 0;
            if (playerCount > 0)
                potsCount = newPotData[0].Length;


            potData.Children.Clear();
            potData.Rows = playerCount + 2;
            potData.Columns = potsCount + 2;
            for (int i = 0; i < potsCount; ++i)
            {
                TextBlock addition = new TextBlock();
                addition.Text = (i + 1).ToString();
                potData.Children.Add(addition);
            }
            TextBlock playerSum = new TextBlock();
            playerSum.Text = "Player Total";
            potData.Children.Add(playerSum);
            IEnumerator<Player> enumerator = playerOrder.GetEnumerator();
            int[] potSum = new int[potsCount];
            int sum = 0;
            for (int i = 0; i < playerCount; ++i)
            {
                enumerator.MoveNext();
                TextBlock playerName = new TextBlock();
                // TODO - fix it after the pot information is not implemented as a table.
                if (enumerator.Current == null)
                    return;
                playerName.Text = enumerator.Current.Name;
                potData.Children.Add(playerName);
                sum = 0;
                for (int j = 0; j < potsCount; ++j)
                {
                    TextBlock addition = new TextBlock();
                    addition.Text = string.Format("{0}$", newPotData[i][j]);
                    sum += newPotData[i][j];
                    potSum[j] += newPotData[i][j];
                    potData.Children.Add(addition);
                }
                playerSum = new TextBlock();
                playerSum.Text = string.Format("{0}$", sum);
                playerSum.FontWeight = FontWeights.Bold;
                potData.Children.Add(playerSum);

            }
            sum = 0;
            potData.Children.Add(new TextBlock());
            for (int i = 0; i < potsCount; ++i)
            {
                playerSum = new TextBlock();
                playerSum.Text = string.Format("{0}$", potSum[i]);
                playerSum.FontWeight = FontWeights.Bold;
                potData.Children.Add(playerSum);
                sum += potSum[i];
            }
            TextBlock completeSum = new TextBlock();
            completeSum.Text = string.Format("{0}$", sum);
            potData.Children.Add(completeSum);
        }
    }
}
