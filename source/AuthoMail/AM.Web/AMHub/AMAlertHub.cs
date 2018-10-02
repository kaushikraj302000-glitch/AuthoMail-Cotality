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
            t.Interval = 1000 * 60 * Config.Web_API_AlertElapsedMinute;
            t.Elapsed += TimerElapsed;
            t.Start();
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<AMAlertHub>();
            GetAMGlobalStatus();
            hub.Clients.All.getAMDetailStatus(s);
        }
        public void GetStatus()
        {
            if (s == null) GetAMGlobalStatus();
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

        public void GetAMGlobalStatus()
        {
            using (IMonitorBatchesService alertSvc = new MonitorBatchesService())
            {
                var data = alertSvc.GetPendingBatchesToSubmit(30);
                s = new AMGlobalStatus { DelayedProcessBatches = data.DelayedBatches.Count(), DelayedProcessMailPieces = data.DelayedMailPieces.Count(), DelayedAMReportBackBatches = data.DelayedAMReportBackBatches.Count(), DelayedAMReportBackMailPieces = data.DelayedAMReportBackMailPieces.Count(), DelayedVendorReportBackBatches = data.DelayedVendorReportBackBatches.Count(), DelayedVendorReportBackMailPieces = data.DelayedVendorReportBackMailPieces.Count(), MinHour = data.MinHour, MaxHour = data.MaxHour, LastDelayedBatchReceivedOn = data.LastDelayedBatchReceivedOn, LastUpdatedOn = data.LastUpdatedOn, IsReportBackDelay = data.IsReportBackDelay, IsReceivedDelay = data.IsReceivedDelay, IsProcessDelay = data.IsProcessDelay, Status = data.Status, Pulse = data.Pulse };
            }
        }
    }
}