using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace AM.EDelivery
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

        protected override void CreateSigningRoom()
        {
            log.Debug("EDelivery CreateSigningRoom job queue is started.");

            List<EDMailPiece> validatedMailPieces = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.Validated);
            if (validatedMailPieces.Count == 0)
            {
                log.Debug("EDelivery CreateSigningRoom job queue is empty.");
                log.Debug("EDelivery CreateSigningRoom job queue is completed.");
                return;
            }

            foreach (EDMailPiece mp in validatedMailPieces)
            {
                try
                {
                    MailPieceInfo mpi = new MailPieceInfo(mp.ClientCode, Config.Common_AuthoMailRootFolder, mp.MailBatchId, mp.BatchName);
                    mp.SigningRoomName = "{0}-{1}-{2}".Fill(mp.ClientCode, mp.FileName, mp.Id);
                    mp.ExpiryDate = mp.ExpiryDate.HasValue ? mp.ExpiryDate.Value : mp.ReceivedOn.Value.AddDays(7);

                    SigningRoomRequest request = new SigningRoomRequest()
                    {
                        SigningRoomName = mp.SigningRoomName,
                        Description = "",
                        ExpirationDate = mp.ExpiryDate.Value,
                        StartDate = Common.EstToday
                    };

                    request.Documents = new List<DocumentInfo>() {
                        new DocumentInfo()
                        {
                            DocumentData = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(mpi.AMRequestPath, mp.FileName))).ToString(),
                            DocumentName = Path.GetFileNameWithoutExtension(Path.Combine(mpi.AMRequestPath, mp.FileName)),
                            FileName = Path.GetFileName(Path.Combine(mpi.AMRequestPath, mp.FileName))
                        }
                    };

                    request.Participants = new List<Participant>() {
                        new Participant()
                        {
                            FirstName = mp.RcptFirstName,
                            LastName = mp.RcptLastName,
                            InvitationEmail = mp.RcptEmailId
                        }
                    };

                    IEDeliveryAPIService eDeliveryAPISvc = new EDeliveryAPIService();
                    var response = eDeliveryAPISvc.CreateSigningRoom(request).Result;
                    if (response.HasError)
                    {
                        mp.ErrorCode = OutComeCodes.Issue_In_EDelivery;
                        mp.ErrorDesc = response.Error;

                        eDeliverySvc.UpdateEDMailPieceDetails(mp);
                        eDeliverySvc.UpdateEDMailPiecePipelineStatus(mp.Id, EDPipelineStatus.ReportedBack, Common.Now);
                        #region Check IsFallBackRequested
                        if (mp.IsFallBackRequested)
                        {
                            eDeliverySvc.FallBackEDeliveryPackage(mp.Id, mp.UniqueId, mp.MailBatchId, mp.ClientCode);
                        }
                        else
                        {
                            eDeliverySvc.ErrorOutEDeliveryPackage(mp.UniqueId, mp.MailBatchId, mp.ErrorCode, mp.ErrorDesc);
                        }
                        #endregion
                        continue;
                    }
                    else
                    {
                        mp.SigningRoomId = response.SigningRoom.SigningRoomId;
                        mp.SigningRoomUniqueId = response.SigningRoom.SigningRoomGuid;
                        mp.ExpiryDate = response.SigningRoom.ExpirationDate;

                        eDeliverySvc.UpdateEDMailPieceDetails(mp);
                        eDeliverySvc.UpdateEDMailPiecePipelineStatus(mp.Id, EDPipelineStatus.SigningRoomCreated, Common.Now);
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while creating signing room for Id {0} for client {1} with below error <br/>{2}".Fill(mp.Id, mp.ClientCode, ex);
                    log.Error(msg);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            log.Debug("EDelivery CreateSigningRoom job queue is completed.");
        }

        protected override void SendInvitations()
        {
            log.Debug("EDelivery SendInvitations job queue is started.");

            List<EDMailPiece> signingRoomCreatedMailPieces = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.SigningRoomCreated);

            if (signingRoomCreatedMailPieces.Count == 0)
            {
                log.Debug("EDelivery SendInvitations job queue is empty.");
                log.Debug("EDelivery SendInvitations job queue is completed.");
                return;
            }

            foreach (EDMailPiece mp in signingRoomCreatedMailPieces)
            {
                try
                {
                    IEDeliveryAPIService eDeliveryAPISvc = new EDeliveryAPIService();
                    bool isInvitationSent = eDeliveryAPISvc.SendInvitations(mp.SigningRoomId.Value).Result;

                    if (isInvitationSent)
                    {
                        eDeliverySvc.UpdateEDMailPiecePipelineStatus(mp.Id, EDPipelineStatus.InvitationSent, Common.Now);
                    }
                    else
                    {
                        mp.ErrorCode = OutComeCodes.Issue_In_EDelivery;
                        mp.ErrorDesc = "Error occured while sending invitation to participant for mail piece Id {0}".Fill(mp.Id);

                        eDeliverySvc.UpdateEDMailPieceDetails(mp);
                        eDeliverySvc.UpdateEDMailPiecePipelineStatus(mp.Id, EDPipelineStatus.ReportedBack, Common.Now);
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while sending invitation to participant Id {0} for client {1} with below error <br/>{2}".Fill(mp.Id, mp.ClientCode, ex);
                    log.Error(msg);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            log.Debug("EDelivery SendInvitations job queue is completed.");
        }

        protected override void EDelivered()
        {
            log.Debug("EDelivery ReportPrepped job queue is started.");

            List<EDMailPiece> eDeliveredMPs = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.eDelivered);

            if (eDeliveredMPs.Count == 0)
            {
                log.Debug("EDelivery ReportPrepped job queue is empty.");
                log.Debug("EDelivery ReportPrepped job queue is completed.");
                return;
            }

            foreach (EDMailPiece edmp in eDeliveredMPs)
            {
                try
                {
                    using (IMailPieceService mpSvc = new MailPieceService())
                    using (IReportService reportSvc = new ReportService())
                    {
                        MailPieces mailPiece = reportSvc.GetMailPiecesDataFromMailBatchIdAndUniqueId(edmp.MailBatchId, edmp.UniqueId);
                        mailPiece.PrintedOn = edmp.DocDownloadedOn.Value;
                        mailPiece.ShippedOn = edmp.DocDownloadedOn.Value;
                        mailPiece.ShippingStatus = ShippingStatus.Shipped.ToString();

                        mpSvc.UpdateBatchMailPieceShippingStatusDetails(mailPiece);
                        MailPieceInfo mpi = new MailPieceInfo(edmp.ClientCode, Config.Common_AuthoMailRootFolder, edmp.MailBatchId, edmp.BatchName);
                        GenerateDeliveryReport(edmp, mailPiece, mpi);
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while preparing report for Id {0} for client {1} with below error <br/>{2}".Fill(edmp.Id, edmp.ClientCode, ex);
                    log.Error(msg);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            log.Debug("EDelivery ReportPrepped job queue is completed.");
        }//Check Edelivered //Check Expired

        private void GenerateDeliveryReport(EDMailPiece eDeliveryMP, MailPieces mp, MailPieceInfo mpi)
        {
            string amResponsePath = mpi.AMResponsePath;

            switch (eDeliveryMP.ChannelCode.ToUpper())
            {
                case "AMC":
                    using (IAMCService amcSvc = new AMCService())
                    {
                        StringBuilder deliveryReport = amcSvc.AMCDeliveryReport(new List<MailPieces>() { mp });

                        if (deliveryReport != null)
                        {
                            string deliveryReportFileName = "Delivery-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));

                            string deliveryReportPath = Path.Combine(amResponsePath, deliveryReportFileName);

                            File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                        }
                    }
                    Thread.Sleep(1000);
                    break;
                default:
                    var deliverables = commonSvc.GetDeliverableByReportType(eDeliveryMP.ClientId.Value, ReportTypes.Delivery);
                    using (IGeneralService generalSvc = new GeneralService())
                    {
                        StringBuilder deliveryReport = generalSvc.GeneralDeliveryReport(new List<MailPieces>() { mp }, deliverables.Delimeter);

                        if (deliveryReport != null)
                        {
                            string deliveryReportFileName = "Delivery-{0}-{1}-{2}-{3}.txt".Fill(eDeliveryMP.BatchName, eDeliveryMP.ClientCode, eDeliveryMP.MailBatchId, Common.Now.ToString("yyyyMMdd-HHmmss"));
                            string deliveryReportPath = Path.Combine(amResponsePath, deliveryReportFileName);

                            File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                        }
                    }
                    break;
            }
        }

        protected override void Expired()
        {
            log.Debug("EDelivery ReportPrepped job queue is started.");

            List<EDMailPiece> eDeliveredMPs = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.eDelivered);

            if (eDeliveredMPs.Count == 0)
            {
                log.Debug("EDelivery ReportPrepped job queue is empty.");
                log.Debug("EDelivery ReportPrepped job queue is completed.");
                return;
            }

            foreach (EDMailPiece edmp in eDeliveredMPs)
            {
                try
                {
                    using (IMailPieceService mpSvc = new MailPieceService())
                    using (IReportService reportSvc = new ReportService())
                    {
                        MailPieces mailPiece = reportSvc.GetMailPiecesDataFromMailBatchIdAndUniqueId(edmp.MailBatchId, edmp.UniqueId);
                        mailPiece.PrintedOn = edmp.DocDownloadedOn.Value;
                        mailPiece.ShippedOn = edmp.DocDownloadedOn.Value;
                        mailPiece.ShippingStatus = ShippingStatus.Shipped.ToString();

                        mpSvc.UpdateBatchMailPieceShippingStatusDetails(mailPiece);
                        MailPieceInfo mpi = new MailPieceInfo(edmp.ClientCode, Config.Common_AuthoMailRootFolder, edmp.MailBatchId, edmp.BatchName);
                        GenerateDeliveryReport(edmp, mailPiece, mpi);
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while preparing report for Id {0} for client {1} with below error <br/>{2}".Fill(edmp.Id, edmp.ClientCode, ex);
                    log.Error(msg);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            log.Debug("EDelivery ReportPrepped job queue is completed.");
        }
    }
}
