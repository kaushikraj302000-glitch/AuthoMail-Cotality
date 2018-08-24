using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using AM.Lib;
using AM.Services;
using AM.Entities;
using System.Net.Mail;
using System.IO;

namespace AM.Reporting
{
    class DallasNotification
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static void GenerateEmailNotification()
        {
            var dallasCstTimeSpan = Array.ConvertAll(AppConfig.DallasNotificationTimings.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), s => TimeSpan.Parse(s));
            var currentCstDateTime = DateTime.UtcNow.UtcToClientTimeZone(AppConfig.DallasTimeZone);
            var currentCstDate = currentCstDateTime.Date;
            var _05PMSweep = currentCstDate + dallasCstTimeSpan[0];
            var _06PMSweep = currentCstDate + dallasCstTimeSpan[1];
            var _07PMSweep = currentCstDate + dallasCstTimeSpan[2];
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();
            DateTime effectiveDateTime = new DateTime();
            if (currentCstDateTime < _05PMSweep)
            {
                startTime = currentCstDate.ClientTimeZoneToUtc(AppConfig.DallasTimeZone);
                endTime = _05PMSweep.ClientTimeZoneToUtc(AppConfig.DallasTimeZone);
                effectiveDateTime = _05PMSweep;
            }
            else if (currentCstDateTime >= _05PMSweep && currentCstDateTime < _06PMSweep)
            {
                startTime = _05PMSweep.ClientTimeZoneToUtc(AppConfig.DallasTimeZone);
                endTime = _06PMSweep.ClientTimeZoneToUtc(AppConfig.DallasTimeZone);
                effectiveDateTime = _06PMSweep;
            }
            else if (currentCstDateTime >= _06PMSweep && currentCstDateTime < _07PMSweep)
            {
                startTime = _06PMSweep.ClientTimeZoneToUtc(AppConfig.DallasTimeZone);
                endTime = _07PMSweep.ClientTimeZoneToUtc(AppConfig.DallasTimeZone);
                effectiveDateTime = _07PMSweep;
            }
            else
            {
                throw new Exception("Unexpected Error Occured While sending an Email");
            }

            List<MailPieces> dallasMailPieces;
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            {
                dallasMailPieces = mailPieceSvc.GetDallasMailPiecesByTimingsAndClient(startTime, endTime);
            }

            if (dallasMailPieces != null)
            {
                var clients = dallasMailPieces.Select(c => c.Clients.AppCode).Distinct();
                List<Notification> notificationList = new List<Notification>();
                foreach (string clientCode in clients)
                {
                    var mailPiecesByClient = dallasMailPieces.Where(f => f.Clients.AppCode == clientCode).ToList();

                    var shippingServiceTypes = mailPiecesByClient.Select(c => c.DocTypes.ShippingServiceType).Distinct();

                    foreach (string shippingServiceType in shippingServiceTypes)
                    {
                        string shippingCarrier = shippingServiceType;
                        if (shippingServiceType == "PRIORITY_OVERNIGHT")
                            shippingCarrier = "FedEx";
                        var mailPiecesByShippingServiceType = mailPiecesByClient.Where(f => f.DocTypes.ShippingServiceType == shippingServiceType).ToList();
                        notificationList.Add(new Notification() { ClientCode = clientCode, ShippingType = shippingCarrier, MailPieceCount = mailPiecesByShippingServiceType.Count });
                    }
                }

                SendEmailNotification(notificationList, effectiveDateTime);
            }
        }

        private static void SendEmailNotification(List<Notification> notificationList, DateTime currentCstDate)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(AppConfig.DallasEMailTo);
                        msg.CC.Add(AppConfig.DallasEMailCc);
                        msg.Subject = "{0} : {1} {2}".Fill(Config.Common_Environment, "Dallas", AppConfig.DallasEMailSubject);
                        msg.Priority = MailPriority.Normal;

                        var clientList = notificationList.Select(c => c.ClientCode).Distinct();

                        StringBuilder fullContent = new StringBuilder();
                        foreach (var client in clientList)
                        {
                            fullContent.AppendLine(@"<tr><td colspan='3' style='width:6.65in;border:solid windowtext 1.0pt;padding:0in 5.4pt 0in 5.4pt' valign='top' width='638'><p style='text-align:center' align='center'><span style='font-size:20.0pt'>
                                    {0} / {1} / {2}
                                    <u></u><u></u>
                                </span></p></td></tr>".Fill(client, currentCstDate.ToString("MMddyyyy"), currentCstDate.ToString("hhtt")));
                            fullContent.AppendLine(@"<tr><td style='width:6.65in;border:solid windowtext 1.0pt;padding:0in 6pt 0in 6pt;text-align:center;font-size:14.0pt' valign='top' width='638'>Shipping Method</td><td style='width:6.65in;border:solid windowtext 1.0pt;padding:0in 6pt 0in 6pt;text-align:center;font-size:14.0pt' valign='top' width='638'>Total Packages Count</td></tr>");

                            foreach (var filterd in notificationList.Where(f => f.ClientCode == client))
                            {
                                fullContent.AppendLine(@"<tr><td><p style='text-align:center' align='center'><span style='font-size:14.0pt'>{0}</span></p></td><td><p style='text-align:center' align='center'>
                                <span style='font-size:14.0pt'>{1}</span></p></td></tr>".Fill(filterd.ShippingType, filterd.MailPieceCount));
                            }
                        }

                        StringBuilder sb = new StringBuilder(File.ReadAllText(AppConfig.DallasEMailTemplate));
                        sb = sb.Replace("<%REPLACE%>", fullContent.ToString());

                        smtpClient.Send(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while sending an Email to Dallas Team : ", ex);
            }
        }
    }

    class Notification
    {
        public string ClientCode { get; set; }
        public string ShippingType { get; set; }
        public int MailPieceCount { get; set; }
    }
}
