using AM.Entities;
using AM.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    public class DownloadJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DownloadJob));

        IClientService clientSvc;
        public DownloadJob()
        {
            clientSvc = new ClientService();
        }
        public void DownloadNewBatch()
        {
            List<Client> clientList = clientSvc.GetClientList(null, false);

            foreach (var client in clientList)
            {
                SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(client.SFTProtocol), client.SFTHost, client.SFTUserName, client.SFTPassword, 300000);
                sftSite.Connect();

                if (sftSite.IsDownloadDirectoryAvailable(client.SFTADERequestDir))
                {
                    var listFiles = sftSite.ListDirectory(client.SFTADERequestDir).Where(x => Path.GetExtension(x).Equals(".xml", StringComparison.CurrentCultureIgnoreCase)).ToList();
                    if (listFiles != null && listFiles.Count > 0)
                    {
                        foreach (var file in listFiles)
                        {
                            
                        }
                    }
                }
            }

        }
    }
}
