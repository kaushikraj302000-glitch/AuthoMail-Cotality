using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System.Configuration;
using System.Net.Mail;
using System.IO;
using AM.Services.USPS.TrackV2;
using AM.USPSLib.TrackV2;

namespace AM.Reporting
{
    class CarrierTrackingInfo
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static void CheckForShipmentTracking()
        {
            try
            {
                log.Info("FedEx Pickup Process Started");
                List<FedExReport> mailPieces = new List<FedExReport>();
                using (IFedExService fedExTrackingSvc = new FedExService())
                using (IReportService reportSvc = new ReportService())
                {
                    DateTime dueDateStartTime = Common.Now.UtcToEst().AddDays(-10).ToStartDateTime();
                    //Here we are passing dates in EST format because we are looking for est dates column
                    DateTime dueDateEndTime = Common.Now.UtcToEst().ToEndDateTime();
                    log.Info("FedEx Tracking Process For Date And Time {0}".Fill(dueDateEndTime));
                    //UPDATE Pickupon list before generating reports.
                    fedExTrackingSvc.UpdateFedExShipmentInfo(dueDateStartTime, dueDateEndTime);
                }
                log.Info("FedEx Pickup Process Completed");
            }
            catch (Exception ex)
            {
                log.Error("Error occured while checkign for FedEx packages picked up or not", ex);
            }
        }

        internal static void CheckForUSPSeVSShipmentTracking()
        {
            try
            {
                log.Info("USPS eVS Tracking Process Started");

                using (IUSPSService uspsSvc = new USPSService())
                using (ICommonService commonSvc = new CommonService())
                using (IReportService reportSvc = new ReportService())
                {
                    DateTime dueDateStartTime = Common.Now.UtcToEst().AddDays(-10).ToStartDateTime();
                    //Here we are passing dates in EST format because we are looking for est dates column
                    DateTime dueDateEndTime = Common.Now.UtcToEst().ToEndDateTime();
                    log.Info("USPS Tracking Process For Date And Time {0}".Fill(dueDateEndTime));

                    List<USPSeVSReport> mailPieces = reportSvc.GetUSPSeVSDataForTracking(dueDateStartTime, dueDateEndTime);

                    if (mailPieces == null || mailPieces.Count == 0)
                        return;


                    int requestTrackingCount = mailPieces.Count;
                    int maxTrackRequest = 35;
                    int trackedRequests = 0;
                    double loopCount = Math.Ceiling(requestTrackingCount / (float)maxTrackRequest);

                    for (double i = 0; i < loopCount; i++)
                    {
                        List<TrackID> trackIDs = new List<TrackID>();
                        mailPieces.Skip(trackedRequests).Take(maxTrackRequest).ToList().ForEach(r =>
                        {
                            trackIDs.Add(new TrackID() { ID = r.TrackingNo });
                        });

                        TrackFieldAPIRequest trackFieldRequest = new TrackFieldAPIRequest()
                        {
                            TrackID = trackIDs
                        };

                        var trackingInfos = uspsSvc.GetTrackingDetailsAsync(commonSvc.GetSystemUserId(), trackFieldRequest).Result;

                        if (trackingInfos.HasError)
                        {
                            log.Error($"Error occured while tracking shipment info for USPS {trackingInfos.Error}");
                            continue;
                        }

                        trackedRequests += maxTrackRequest;

                        using (IMailPieceService mailPieceSvc = new MailPieceService())
                        {
                            foreach (var trackInfo in trackingInfos.TrackInfo)
                            {
                                log.Debug(trackInfo.ID);
                                if (trackInfo == null || trackInfo.TrackSummary == null)
                                {
                                    log.Error("Tracking info not found.");
                                    continue;
                                }
                                var shipmentDateTimeEst = (trackInfo.TrackSummary.EventDate + " " + trackInfo.TrackSummary.EventTime).Trim().AnyFormatToDateTime();

                                var shipmentDateTimeUTC = shipmentDateTimeEst.HasValue ? (DateTime?)shipmentDateTimeEst.Value.EstToUtc() : null;

                                var shipmentStatus = $"{trackInfo.TrackSummary.Event} on {shipmentDateTimeUTC} UTC at ({trackInfo.TrackSummary.EventCity},{trackInfo.TrackSummary.EventState})";

                                mailPieceSvc.UpdateMailPieceShipmentStatusFromTrackingNo(shipmentStatus, trackInfo.ID, Common.Now, trackInfo.TrackSummary.Event, shipmentDateTimeUTC);
                            }
                        }
                    }
                }
                log.Info("USPS eVS Tracking Process Completed");
            }
            catch (Exception ex)
            {
                log.Error("Error occured while checking for USPS packages picked up or not", ex);
            }
        }
    }
}
