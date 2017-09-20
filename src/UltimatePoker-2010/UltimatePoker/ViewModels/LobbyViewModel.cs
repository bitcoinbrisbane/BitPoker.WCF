using PokerService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace UltimatePoker.ViewModels
{
    public class LobbyViewModel
    {
        private DiscoveryHelper helper = new DiscoveryHelper();

        private DispatcherTimer refreshTimer = new DispatcherTimer();

        private ObservableCollection<ServiceLocation> discoveredServers = new ObservableCollection<ServiceLocation>();

        public ObservableCollection<ServiceLocation> DiscoveredServers
        {
            get { return discoveredServers; }
        }

        public LobbyViewModel()
        {
            refreshTimer.Interval = TimeSpan.FromMilliseconds(500);
            refreshTimer.Tick += new EventHandler(refreshTimer_Tick);
        }

        public void StartTimer()
        {
            refreshTimer.Start();
        }

        void refreshTimer_Tick(object sender, EventArgs e)
        {
            Func<IEnumerable<ServiceLocation>> call = new Func<IEnumerable<ServiceLocation>>(helper.Discover);
            call.BeginInvoke(OnDiscovered, call);
            refreshTimer.IsEnabled = false;
        }

        private void OnDiscovered(IAsyncResult result)
        {
            try
            {
                Func<IEnumerable<ServiceLocation>> call = (Func<IEnumerable<ServiceLocation>>)result.AsyncState;
                IEnumerable<ServiceLocation> discovered = call.EndInvoke(result);

                int prevCount = discoveredServers.Count;
                discoveredServers.Clear();

                foreach (ServiceLocation msg in discovered)
                {
                    discoveredServers.Add(msg);
                }

                if (prevCount != discoveredServers.Count || discoveredServers.Count == 0)
                {
                    refreshTimer.Interval = TimeSpan.FromMilliseconds(500);
                }
                else if (refreshTimer.Interval.TotalMilliseconds < 30000)
                {
                    refreshTimer.Interval = refreshTimer.Interval + refreshTimer.Interval;
                }
            }
            catch
            {
                refreshTimer.Stop();
            }
        }
    }
}
