using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;

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
            //No need to implement
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
                    SigningRoomRequest request = new SigningRoomRequest()
                    {
                        SigningRoomName = mp.SigningRoomName,
                        Description = "",
                        ExpirationDate = mp.ExpiryDate.HasValue ? mp.ExpiryDate.Value : mp.ReceivedOn.Value.AddDays(7),
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
                        eDeliverySvc.UpdateEDMailPiecePipelineStatus(mp.Id, EDPipelineStatus.Expired, Common.Now);
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
                        eDeliverySvc.UpdateEDMailPiecePipelineStatus(mp.Id, EDPipelineStatus.Expired, Common.Now);
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

        protected override void CheckActivityLogs()
        {
            log.Debug("EDelivery Check ActivityLogs job queue is started.");

            List<EDMailPiece> invitationSentMailPieces = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.InvitationSent);

            if (invitationSentMailPieces.Count == 0)
            {
                log.Debug("EDelivery Check ActivityLogs job queue is empty.");
                log.Debug("EDelivery Check ActivityLogs job queue is completed.");
                return;
            }

            foreach (EDMailPiece mp in invitationSentMailPieces)
            {
                try
                {
                    IEDeliveryAPIService eDeliveryAPISvc = new EDeliveryAPIService();
                    var activityLogs = eDeliveryAPISvc.GetSigningRoomActivityLog(mp.SigningRoomName).Result;

                    string signingRoomStatus = eDeliveryAPISvc.GetSigningRoomStatus(mp.SigningRoomId.Value).Result;

                    //NOTE : Here system will check if signing room is expired and activity logs have 3 activities only which were created at signing room creating process then system will consider this as Expired.
                    if (signingRoomStatus == SigningRoomStatus.Expired && activityLogs.Count == 3)
                    {
                        //eDeliverySvc.UpdateEDMailPiecePipelineStatus(mp.Id, EDPipelineStatus.Expired, Common.Now);
                        #region Check IsFallBackRequested
                        if (mp.IsFallBackRequested)
                        {
                            eDeliverySvc.FallBackEDeliveryPackage(mp.Id, mp.UniqueId, mp.MailBatchId, mp.ClientCode);
                        }
                        else
                        {
                            mp.ErrorCode = OutComeCodes.Issue_In_EDelivery;
                            mp.ErrorDesc = "Signing room is expired without any activity done by participant.";
                            mp.IsExpired = true;
                            eDeliverySvc.UpdateEDMailPieceDetails(mp);
                            eDeliverySvc.ErrorOutEDeliveryPackage(mp.UniqueId, mp.MailBatchId, mp.ErrorCode, mp.ErrorDesc);
                        }
                        #endregion
                    }


                    foreach (var actlog in activityLogs)
                    {
                        switch (actlog.EDeliveryStatus)
                        {
                            case EDeliveryStatus.ESignConsentAccepted:
                                eDeliverySvc.UpdateEDMailPiecePipelineStatus(mp.Id, EDPipelineStatus.Consented, actlog.UpdatedDate);
                                break;
                            case EDeliveryStatus.ESignConsentDeclined:
                                eDeliverySvc.UpdateEDMailPiecePipelineStatus(mp.Id, EDPipelineStatus.NotConsented, actlog.UpdatedDate);
                                break;
                            case EDeliveryStatus.DocumentReviewed:
                                eDeliverySvc.UpdateEDMailPiecePipelineStatus(mp.Id, EDPipelineStatus.DocReviewed, actlog.UpdatedDate);
                                break;
                            case EDeliveryStatus.DocumentDownloaded:
                                eDeliverySvc.UpdateEDMailPiecePipelineStatus(mp.Id, EDPipelineStatus.DocDownloaded, actlog.UpdatedDate);
                                eDeliverySvc.UpdateEDMailPiecePipelineStatus(mp.Id, EDPipelineStatus.eDelivered, actlog.UpdatedDate);
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while checking for activity logs of Id {0} for client {1} with below error <br/>{2}".Fill(mp.Id, mp.ClientCode, ex);
                    log.Error(msg);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            log.Debug("EDelivery Check ActivityLogs job queue is started.");
        }
    }
}
