using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Data.SqlClient;
using AM.USPSLib;
using System.Transactions;
using System.Threading;

namespace AM.Console
{
    class EDGeneralChannel : EDChannel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected IGeneralService generalSvc;

        private string channelType;

        public EDGeneralChannel(string channelCode, string channelType)
            : base(channelCode)
        {
            generalSvc = GetService<GeneralService>();
            this.channelType = channelType;
        }

        protected override void EnumerateBatches()
        {
            //No need to implement
        }

        protected override void DownloadBatches()
        {
            //No need to implement
        }

        protected override void ValidateBatches()
        {
            log.Debug("EDelivery Validate job queue is started.");

            List<EDMailPiece> downloadedMailPieces = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.Downloaded);
            if (downloadedMailPieces == null)
            {
                log.Debug("EDelivery Validate job queue is empty.");
                return;
            }

            foreach (var mp in downloadedMailPieces)
            {
                string mpRootPath = Path.Combine(Config.Common_EDeliveryRootFolder, mp.Clients.AppCode, mp.Id.ToString());

                string filePath = Path.Combine(mpRootPath, mp.FileName);

                DocType mpDocumentType = eDeliverySvc.GetEDDocTypeByDocTypeName(mp.ClientId.Value, mp.DocumentOrPackageType);

                if (!File.Exists(filePath))
                {
                    mp.ErrorCode = OutComeCodes.Missing_PDF_File;
                    mp.ErrorDesc = "Pdf file is not available for this package.";
                    mp.PipelineStatus = EDPipelineStatus.ErroredOut;
                }
                else if (string.IsNullOrWhiteSpace(mp.RcptEmailId))
                {
                    mp.ErrorCode = OutComeCodes.Recipient_Details_Missing_Or_Invalid;
                    mp.ErrorDesc = "Recipient email address is not exist for E-Delivery.";
                    mp.PipelineStatus = EDPipelineStatus.FallBack;
                }
                else if (mpDocumentType == null)
                {
                    mp.ErrorCode = OutComeCodes.Invalid_PackageType;
                    mp.ErrorDesc = "Provided Document type {0} is not configured for E-Delivery.".Fill(mp.DocumentOrPackageType);
                    mp.PipelineStatus = EDPipelineStatus.FallBack;
                }
                else
                {
                    mp.TotalPages = PdfUtil.GetTotalPageCount(filePath);
                    mp.DocTypeId = mpDocumentType.Id;
                    mp.ExpiryDate = DateTime.Now;//Calculation of Expiry Date
                    mp.PDFFileSize = filePath.GetFileSizeInKB();
                }

                eDeliverySvc.UpdateEDMailPieceDetails(mp);
                eDeliverySvc.UpdateEDMailPiecePipelineStatus(mp.Id, mp.PipelineStatus, Common.Now);
            }

            log.Debug("EDelivery Validate job queue is completed.");
        }

        protected override void FallBackBatches()
        {
            log.Debug("EDelivery FallBack job queue is started.");

            List<EDMailPiece> downloadedMailPieces = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.FallBack);
            if (downloadedMailPieces == null)
            {
                log.Debug("EDelivery FallBack job queue is empty.");
                return;
            }

            log.Debug("EDelivery FallBack job queue is started.");
        }

        protected override void CreateSigningRoom()
        {
            throw new NotImplementedException();
        }

        protected override void SendInvitations()
        {
            throw new NotImplementedException();
        }
    }
}
