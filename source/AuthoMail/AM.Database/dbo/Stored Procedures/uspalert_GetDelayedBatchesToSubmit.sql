CREATE PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit] 	
	@cutOffMinutes int
AS
BEGIN
 SELECT mp.Id,mp.BusinessFriendlyIdValue AS LoanNumber,mp.TrackingNo,mb.Id AS MailBatchId,rq.UniqueId,bs.IsSubmitted,c.AppCode,
 mb.BatchName,mb.ReceivedOn,mb.PipelineStatus,mp.DueDate, mp.ShippingStatus,d.DocTypeName AS DocTypeCode,PCD.ShippingCarrier,PCD.ShippingServiceType,mp.ClientId,mp.PrintLocation,pv.VendorCode, rq.MailPieceStatus,mp.IsEDeliveryRequested,emp.IsInvitationSent,mp.IsFallBackRequested,pt.TrailName AS PrintTrail  
 FROM AM.MailPieces mp
 INNER JOIN AM.MailBatches mb ON mp.MailBatchId=mb.Id
 LEFT OUTER JOIN AM.EDeliveryMailPieces emp ON mp.UniqueId = emp.UniqueId
 LEFT OUTER JOIN Common.PrintTrails pt ON mp.PrintTrailId = pt.Id
 LEFT OUTER JOIN Common.PrintVendors pv ON pt.PrintVendorId = pv.Id
 LEFT OUTER JOIN Common.Clients c ON mb.ClientId=c.Id
 LEFT OUTER JOIN PnS.MRDFRequest rq ON mp.UniqueId=rq.UniqueId
 LEFT OUTER JOIN PnS.BatchSummary bs ON rq.JobId=bs.PrintBatch
 LEFT OUTER JOIN [AM].[DocTypes] d ON mp.DocTypeId=d.Id 
 LEFT OUTER JOIN Common.PnSConfigDetail PCD ON mp.PnSConfigDetailId = PCD.Id
 WHERE mb.ReceivedOn between DATEADD(day,-7,GETUTCDATE()) AND DATEADD(minute,-@cutOffMinutes,GETUTCDATE()) AND (mp.ShippingStatus is NULL or (mp.ShippingStatus in ('InProgress','Shipped') AND (rq.UniqueId is NULL or ISNULL(bs.IsSubmitted,0)=0)) or (mp.ShippingStatus = 'Error' AND mp.ErrorDesc IS NULL) or (mp.ShippingStatus = 'PulledBack' AND not exists (SELECT Id FROM AM.PullBackRequests WHERE MailPieceId = mp.Id)) or (mb.ClientId IS NULL) or (mp.IsEDeliveryRequested = 1 AND emp.IsInvitationSent = 0 AND emp.PipelineStatus <> 'ReportedBack'))
 or (rq.MailPieceStatus='Shipped' AND mp.IsPickedUp <> 1 AND mp.DueDate < GETUTCDATE() AND PCD.ShippingCarrier='FedEx' AND mb.ReceivedOn>=DATEADD(DAY,-30,GETUTCDATE()))
END
    