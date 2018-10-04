using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AM.Services;

namespace AM.Web.AMHub
{
    public class AMAlertHub : Hub
    {
        private AMAlert _amAlert;
        public AMAlertHub()
            : this(AMAlert.Instance)
        {
        }
        public AMAlertHub(AMAlert amAlert)
        {
            _amAlert = amAlert;
        }
        public void GetConnection(string conId)//Get Connection id
        {
            _amAlert.UpdateAMAlert(conId);
        }
    }

}