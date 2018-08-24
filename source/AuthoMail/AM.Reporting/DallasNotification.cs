using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using AM.Lib;
using AM.Services;
using AM.Entities;

namespace AM.Reporting
{
    class DallasNotification
    {
        internal static void SendEmailNotification()
        {
            var dallasCstTimeSpan = Array.ConvertAll(ConfigurationManager.AppSettings["DallasNotificationTimings"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), s => TimeSpan.Parse(s));
            var currentCstDateTime = DateTime.UtcNow.UtcToClientTimeZone("Central Standard Time");
            var currentCstDate = currentCstDateTime.Date;
            var _05PMSweep = currentCstDate + dallasCstTimeSpan[0];
            var _06PMSweep = currentCstDate + dallasCstTimeSpan[1];
            var _07PMSweep = currentCstDate + dallasCstTimeSpan[2];
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();
            if (currentCstDateTime < _05PMSweep)
            {
                startTime = currentCstDate.ClientTimeZoneToUtc("Central Standard Time");
                endTime = _05PMSweep.ClientTimeZoneToUtc("Central Standard Time");
            }
            else if (currentCstDateTime >= _05PMSweep && currentCstDateTime < _06PMSweep)
            {
                startTime = currentCstDate.ClientTimeZoneToUtc("Central Standard Time");
                endTime = _06PMSweep.ClientTimeZoneToUtc("Central Standard Time");
            }
            else if (currentCstDateTime >= _06PMSweep && currentCstDateTime < _07PMSweep)
            {
                startTime = currentCstDate.ClientTimeZoneToUtc("Central Standard Time");
                endTime = _07PMSweep.ClientTimeZoneToUtc("Central Standard Time");
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
                var clients = dallasMailPieces.Select(c => c.ClientId).Distinct();
                string clientCode = string.Empty;
                foreach (int clientId in clients)
                {
                    Client client;
                    using (IClientService clientSvc = new ClientService())
                    {
                        client = clientSvc.GetClientById(clientId);
                    }

                    var mailPiecesByClient = dallasMailPieces.Where(f => f.ClientId == clientId);


                }
            }
        }
    }
}
