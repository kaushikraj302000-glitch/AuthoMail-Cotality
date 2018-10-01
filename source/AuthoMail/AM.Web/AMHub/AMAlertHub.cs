using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using AM.Services;

namespace AM.Web.AMHub
{
    public class AMAlertHub : Hub
    {
        static readonly Timer t = new Timer();
        static int connectedClients = 0;
        static AMGlobalStatus s = null;
        public AMAlertHub()
        {
            int minutes = 1;
            t.Interval = 1000 * 60 * minutes;
            t.Elapsed += TimerElapsed;
            t.Start();
            t.Enabled = true;
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (connectedClients > 0)
            {
                //Send Message to all clients
                var hub = GlobalHost.ConnectionManager.GetHubContext<AMAlertHub>();
                using (IMonitorBatchesService alertSvc = new MonitorBatchesService())
                {
                    int defaultCutOff = 30;
                    var data = alertSvc.GetPendingBatchesToSubmit(defaultCutOff);
                    s = new AMGlobalStatus { DelayedBatches = data.DelayedBatches.Count(), DelayedMailPieces = data.DelayedMailPieces.Count(), MinHour = data.MinHour, MaxHour = data.MaxHour, Status = data.Status, LastBatchReceivedOn = data.LastBatchReceivedOn, LastUpdatedOn = data.LastUpdatedOn };
                    hub.Clients.All.getAMDetailStatus(s);
                }
            }
        }
        public void GetStatus()
        {
            Clients.All.getAMDetailStatus(s);
        }

        public override Task OnConnected()
        {
            connectedClients++;
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool s)
        {
            connectedClients--;
            return base.OnDisconnected(s);
        }

    }
}