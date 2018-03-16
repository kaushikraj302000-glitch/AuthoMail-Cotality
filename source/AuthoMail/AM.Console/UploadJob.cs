using AM.Entities;
using AM.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AM.Lib;
namespace AM.Console
{
    public class UploadJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(UploadJob));
        IClientService clientSvc;
        IBatchService mailBatchSvc;
        IMailPieceService mailPieceSvc;
        ICommonService commonSvc;
        public UploadJob()
        {
            clientSvc = new ClientService();
            mailBatchSvc = new BatchService();
            commonSvc = new CommonService();
            mailPieceSvc = new MailPieceService();
        }
    }
}
