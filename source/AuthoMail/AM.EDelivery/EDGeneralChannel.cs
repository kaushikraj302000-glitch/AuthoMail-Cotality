using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Transactions;

namespace AM.EDelivery
{
    class EDGeneralChannel : EDChannel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected IGeneralService generalSvc;

        public EDGeneralChannel()
            : base()
        {
            generalSvc = GetService<GeneralService>();
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
                    mp.SigningRoomName = "{0}-{1}-{2}".Fill(mp.ClientCode, Path.GetFileNameWithoutExtension(mp.FileName), mp.Id);
                    DateTime expiryDate = mp.ExpiryDate.HasValue ? mp.ExpiryDate.Value : GetDefaultExpiryDate(mp.ReceivedOn.Value);//Date will be considered as in EST timezone
                    mp.ExpiryDate = expiryDate;

                    SigningRoomRequest request = new SigningRoomRequest()
                    {
                        SigningRoomName = mp.SigningRoomName,
                        Description = "",
                        ExpirationDate = expiryDate.UtcToEst(),
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
                        var msg = "Error occured while creating signing room for Id {0} with Error".Fill(mp.Id) + Environment.NewLine + response.Error;
                        log.Error(msg);
                        MailUtility.SendUnexpectedExceptionEmail(msg);
                        continue;
                    }
                    else
                    {
                        mp.SigningRoomId = response.SigningRoom.SigningRoomId;
                        mp.SigningRoomUniqueId = response.SigningRoom.SigningRoomGuid;
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

        private DateTime GetDefaultExpiryDate(DateTime receivedOn)
        {
            var workingDateTime = commonSvc.GetNextWorkingDay(receivedOn.UtcToEst(), Config.Common_Holidays, false, false);
            return workingDateTime.ToEndDateTime().EstToUtc();
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
                        var msg = "Invitation did not get sent to participant for Id {0}".Fill(mp.Id);
                        log.Error(msg);
                        MailUtility.SendUnexpectedExceptionEmail(msg);
                        continue;
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
            log.Debug("EDelivery EDelivered job queue is started.");

            List<EDMailPiece> eDeliveredMPs = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.eDelivered);

            if (eDeliveredMPs.Count == 0)
            {
                log.Debug("EDelivery EDelivered job queue is empty.");
                log.Debug("EDelivery EDelivered job queue is completed.");
                return;
            }

            foreach (EDMailPiece edmp in eDeliveredMPs)
            {
                try
                {
                    using (IMailPieceService mpSvc = new MailPieceService())
                    using (IReportService reportSvc = new ReportService())
                    {
                        using (TransactionScope scope = new TransactionScope())
                        {
                            try
                            {
                                MailPieces mailPiece = reportSvc.GetMailPiecesDataFromMailBatchIdAndUniqueId(edmp.MailBatchId, edmp.UniqueId);
                                mailPiece.PrintedOn = edmp.DocDownloadedOn.Value;
                                mailPiece.ShippedOn = edmp.DocDownloadedOn.Value;
                                mailPiece.ShippingStatus = ShippingStatus.Shipped.ToString();

                                mpSvc.UpdateBatchMailPieceShippingStatusDetails(mailPiece);
                                MailPieceInfo mpi = new MailPieceInfo(edmp.ClientCode, Config.Common_AuthoMailRootFolder, edmp.MailBatchId, edmp.BatchName);
                                GenerateDeliveryReport(edmp, mailPiece, mpi);

                                DateTime reportPrepTiming = Common.Now;

                                eDeliverySvc.UpdateEDeliveredMPForReportingBack(edmp.MailBatchId, reportPrepTiming);
                                eDeliverySvc.UpdateEDMailPiecePipelineStatus(edmp.Id, EDPipelineStatus.ReportedBack, reportPrepTiming);
                                scope.Complete();
                            }
                            catch (Exception ex)
                            {
                                var msg = "Error occured while generating EDelivered report for EDMailPiece-Id {0} and BatchName='{1}.xml'".Fill(edmp.Id, edmp.BatchName);
                                log.Error(msg, ex);
                                MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while preparing EDelivered report for Id {0} for client {1} with below error <br/>{2}".Fill(edmp.Id, edmp.ClientCode, ex);
                    log.Error(msg);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            log.Debug("EDelivery EDelivered job queue is completed.");
        }

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
                    using (IGeneralService generalSvc = new GeneralService())
                    {
                        string responseReport = generalSvc.GeneralXMLReport(new List<MailPieces>() { mp }, mp.MailBatches);

                        if (!string.IsNullOrWhiteSpace(responseReport))
                        {
                            string deliveryReportFileName = "{0}-{1}-{2}.xml".Fill(mpi.Client.AppCode, mp.MailBatchId, commonSvc.GetReportingSequence());
                            string deliveryReportPath = Path.Combine(amResponsePath, deliveryReportFileName);

                            File.WriteAllText(deliveryReportPath, responseReport);
                        }
                    }
                    break;
            }
        }

        protected override void Expired()
        {
            log.Debug("EDelivery Expired job queue is started.");

            List<EDMailPiece> eDeliveredMPs = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.Expired);

            if (eDeliveredMPs.Count == 0)
            {
                log.Debug("EDelivery Expired job queue is empty.");
                log.Debug("EDelivery Expired job queue is completed.");
                return;
            }

            foreach (EDMailPiece edmp in eDeliveredMPs)
            {
                try
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        try
                        {
                            #region Check IsFallBackRequested
                            if (edmp.IsFallBackRequested)
                            {
                                using (ICommonService commonSvc = new CommonService())
                                using (IMailPieceService mpSvc = new MailPieceService())
                                using (IDocTypeService docTypeSvc = new DocTypeService())
                                {
                                    MailPieceDTO mailPieceDTO = mpSvc.GetMailPieceDataByUniqueIdAndMBId(edmp.UniqueId, edmp.MailBatchId);
                                    DocType mpDocType = docTypeSvc.GetDocTypeById(mailPieceDTO.DocTypeId.Value);
                                    commonSvc.GetUpdatedDueDateAndPrintLocation(Common.Now, mailPieceDTO, mpDocType, edmp.ClientCode);
                                    mpSvc.UpdateBatchMailPieceDetails(mailPieceDTO);
                                    commonSvc.UpdateBatchPipeLineStatus(edmp.MailBatchId, BatchPipelineStatus.Validated, Common.Now);
                                    eDeliverySvc.UpdateEDMailPiecePipelineStatus(edmp.Id, EDPipelineStatus.ReportedBack, Common.Now);
                                }
                            }
                            else
                            {
                                eDeliverySvc.ErrorOutEDeliveryPackage(edmp);
                            }
                            #endregion

                            scope.Complete();
                        }
                        catch (Exception ex)
                        {
                            var msg = "Error occured while processing expired packages EDMailPiece-Id {0} and BatchName='{1}.xml'".Fill(edmp.Id, edmp.BatchName);
                            log.Error(msg, ex);
                            MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while processing expired packages for Id {0} for client {1} with below error <br/>{2}".Fill(edmp.Id, edmp.ClientCode, ex);
                    log.Error(msg);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            log.Debug("EDelivery Expired job queue is completed.");
        }
    }
}
