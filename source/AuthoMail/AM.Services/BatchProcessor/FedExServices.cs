using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Services.FedExTrackShipmentService;
using System.Net;

namespace AM.Services
{
    public interface IFedExService : IDisposable
    {
        FedExAccountInfo GetFedExAccountDetailsByAccountCode(string accountCode);
        void UpdateFedExTrackingInfo();

        void UpdateFedExShipmentInfo(DateTime dueDateStartTime, DateTime dueDateEndTime);
    }
    public class FedExService : ServiceBase, IFedExService
    {
        public FedExAccountInfo GetFedExAccountDetailsByAccountCode(string accountCode)
        {
            string query = @"SELECT [AccountCode],[WebAuthenticationKey],[WebAuthenticationPassword],[AccountNumber],[MeterNumber] FROM [AM].[ShippingAccounts] WHERE AccountCode=@AccountCode";

            return db.QuerySingleOrDefault<FedExAccountInfo>(query, new { AccountCode = accountCode });
        }

        public void UpdateFedExTrackingInfo()//Value will be 0 for not picked up , 1 for picked up and -1 for both
        {
            List<FedExReport> mailPieces;
            using (IReportService reportSvc = new ReportService())
            {
                mailPieces = reportSvc.GetFedExData(new int[] { 0, -1 });
            }

            if (mailPieces == null || mailPieces.Count == 0)
                return;

            int totalMailPieceCount = mailPieces.Count;
            int checkedMailPieceCount = 0;
            double loopCount = Math.Ceiling(mailPieces.Count / 30.0f);

            using (IFedExService fedExSvc = new FedExService())
            {
                FedExAccountInfo fedExAccountDetails = fedExSvc.GetFedExAccountDetailsByAccountCode(Config.Common_FedExTrackingAccountCode);
                if (fedExAccountDetails != null)
                {
                    for (double i = 0; i < loopCount; i++)
                    {
                        var takenMailPieces = mailPieces.Skip(checkedMailPieceCount).Take(30).ToList();
                        checkedMailPieceCount += 30;

                        TrackRequest request = CreateTrackRequest(takenMailPieces, fedExAccountDetails);
                        TrackPortTypeClient service = new TrackPortTypeClient();
                        service.Endpoint.Address = new System.ServiceModel.EndpointAddress(Config.Common_FedExTrackingUrl);

                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        // Call the Track web service passing in a TrackRequest and returning a TrackReply
                        TrackReply reply = service.track(request);
                        if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                        {
                            GetTrackReplyAndUpdateValue(reply);
                        }
                    }
                }
            }
        }

        private void GetTrackReplyAndUpdateValue(TrackReply reply)
        {
            List<FedExPickedPackagesDetail> fedExPickedPackageDetails = new List<FedExPickedPackagesDetail>();
            List<FedExNotPickedPackagesDetail> fedExNotPickedPackageDetails = new List<FedExNotPickedPackagesDetail>();
            // Track details for each package
            foreach (CompletedTrackDetail completedTrackDetail in reply.CompletedTrackDetails)
            {
                foreach (TrackDetail trackDetail in completedTrackDetail.TrackDetails)
                {
                    if (trackDetail.Events != null)
                    {
                        var trackEvent = trackDetail.Events.Where(c => c.EventType == "PU" || c.EventType == "DL").FirstOrDefault();
                        if (trackEvent != null)
                        {
                            FedExPickedPackagesDetail pickupDetail = new FedExPickedPackagesDetail();
                            pickupDetail.TrackingNo = trackDetail.TrackingNumber;
                            pickupDetail.PickedTimeStamp = trackEvent.Timestamp.ToUniversalTime();
                            fedExPickedPackageDetails.Add(pickupDetail);
                        }
                    }
                }
            }

            using (IMailPieceService mailPieceSvc = new MailPieceService())
            {
                foreach (var pickedUpDetail in fedExPickedPackageDetails)
                {
                    int retVal = mailPieceSvc.UpdateMailPiecePickedUpDetailsFromTrackingNo(pickedUpDetail);
                }
            }
        }

        private void GetTrackReplyAndUpdateValue(TrackReply reply, DateTime jobRunTime)
        {
            List<FedExPickedPackagesDetail> fedExPickedPackageDetails = new List<FedExPickedPackagesDetail>();
            // Track details for each package
            foreach (CompletedTrackDetail completedTrackDetail in reply.CompletedTrackDetails)
            {
                foreach (TrackDetail trackDetail in completedTrackDetail.TrackDetails)
                {
                    if (trackDetail.Events != null)
                    {
                        var trackEvent = trackDetail.Events.FirstOrDefault();
                        if (trackEvent != null)
                        {
                            //todo change in class make separate props Delivery Status and the Delivery Date
                            FedExPickedPackagesDetail pickupDetail = new FedExPickedPackagesDetail();
                            pickupDetail.TrackingNo = trackDetail.TrackingNumber;
                            pickupDetail.PickedTimeStamp = trackEvent.Timestamp.ToUniversalTime();
                            //todo separate this data to new prop Delivery Status and the Delivery Date
                            pickupDetail.DeliveryStatus = trackEvent.EventDescription;
                            pickupDetail.DeliveryDate = trackEvent.Timestamp.ToUniversalTime();
                            pickupDetail.ShipmentStatus = $"{trackEvent.EventDescription} on {trackEvent.Timestamp.ToUniversalTime()} UTC at ( {trackEvent.Address.City}, { trackEvent.Address.StateOrProvinceCode} )";
                            fedExPickedPackageDetails.Add(pickupDetail);
                        }
                    }
                }
            }

            using (IMailPieceService mailPieceSvc = new MailPieceService())
            {
                foreach (var pickedUpDetail in fedExPickedPackageDetails)
                {
                    mailPieceSvc.UpdateMailPieceShipmentStatusFromTrackingNo(pickedUpDetail.ShipmentStatus, pickedUpDetail.TrackingNo, jobRunTime, pickedUpDetail.DeliveryStatus, pickedUpDetail.DeliveryDate);
                }
            }
        }

        private static TrackRequest CreateTrackRequest(List<FedExReport> takenMailPieces, FedExAccountInfo fedExAccountDetails)
        {
            TrackRequest request = new TrackRequest();
            //Credentials
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = fedExAccountDetails.WebAuthenticationKey;
            request.WebAuthenticationDetail.UserCredential.Password = fedExAccountDetails.WebAuthenticationPassword;

            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = fedExAccountDetails.AccountNumber;
            request.ClientDetail.MeterNumber = fedExAccountDetails.MeterNumber;

            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = Config.Common_FedExTrackingAccountCode;

            request.Version = new VersionId();
            List<TrackSelectionDetail> trackSelections = new List<TrackSelectionDetail>();
            // Tracking information
            foreach (var mailPiece in takenMailPieces)
            {
                TrackSelectionDetail trackSelection = new TrackSelectionDetail();
                trackSelection.PackageIdentifier = new TrackPackageIdentifier();
                trackSelection.PackageIdentifier.Value = mailPiece.TrackingNo;
                trackSelection.PackageIdentifier.Type = TrackIdentifierType.TRACKING_NUMBER_OR_DOORTAG;
                trackSelections.Add(trackSelection);
            }
            request.SelectionDetails = trackSelections.ToArray();

            request.ProcessingOptions = new TrackRequestProcessingOptionType[1];
            request.ProcessingOptions[0] = TrackRequestProcessingOptionType.INCLUDE_DETAILED_SCANS;
            return request;
        }

        public void UpdateFedExShipmentInfo(DateTime dueDateStartTime, DateTime dueDateEndTime)
        {
            List<FedExReport> mailPieces;
            using (IReportService reportSvc = new ReportService())
            {
                mailPieces = reportSvc.GetFedExDataForTracking(dueDateStartTime, dueDateEndTime);
            }

            if (mailPieces == null || mailPieces.Count == 0)
                return;

            DateTime jobRunTime = Common.Now;
            int totalMailPieceCount = mailPieces.Count;
            int checkedMailPieceCount = 0;
            double loopCount = Math.Ceiling(mailPieces.Count / 30.0f);

            using (IFedExService fedExSvc = new FedExService())
            {
                FedExAccountInfo fedExAccountDetails = fedExSvc.GetFedExAccountDetailsByAccountCode(Config.Common_FedExTrackingAccountCode);
                if (fedExAccountDetails != null)
                {
                    for (double i = 0; i < loopCount; i++)
                    {
                        var takenMailPieces = mailPieces.Skip(checkedMailPieceCount).Take(30).ToList();
                        checkedMailPieceCount += 30;

                        TrackRequest request = CreateTrackRequest(takenMailPieces, fedExAccountDetails);
                        TrackPortTypeClient service = new TrackPortTypeClient();
                        service.Endpoint.Address = new System.ServiceModel.EndpointAddress(Config.Common_FedExTrackingUrl);

                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        // Call the Track web service passing in a TrackRequest and returning a TrackReply
                        TrackReply reply = service.track(request);
                        if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                        {
                            GetTrackReplyAndUpdateValue(reply, jobRunTime);
                        }
                    }
                }
            }
        }
    }
}
