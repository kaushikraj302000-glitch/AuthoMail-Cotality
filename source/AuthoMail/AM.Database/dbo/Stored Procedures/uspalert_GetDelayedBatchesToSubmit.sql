CREATE PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit] 	
	@cutOffMinutes int
AS
BEGIN
 select mp.Id,mp.businessfriendlyidvalue as LoanNumber,mp.TrackingNo,mb.Id as MailBatchId,rq.UniqueId,bs.IsSubmitted,c.AppCode,mb.BatchName,mb.ReceivedOn,mb.PipelineStatus,mp.ShippingStatus,d.DocTypeCode,d.ShippingCarrier,d.ShippingServiceType,mp.ClientId,pv.Name as PrintLocation,pv.VendorCode,rq.MailPieceStatus  
 from AM.MailPieces mp
 inner join AM.MailBatches mb on mp.MailBatchId=mb.Id
 left outer join Common.PrintTrails pt on mp.PrintTrailId = pt.Id
 left outer join Common.PrintVendors pv on pt.PrintVendorId = pv.Id
 left outer join Common.Clients c on mb.ClientId=c.Id
 left outer join PnS.MRDFRequest rq on mp.UniqueId=rq.UniqueId
 left outer join PnS.BatchSummary bs on rq.JobId=bs.PrintBatch
 left outer join AM.DocTypes d on mp.DocTypeId = d.Id
 where mb.ReceivedOn between DATEADD(day,-7,GETUTCDATE()) and DATEADD(minute,-@cutOffMinutes,GETUTCDATE()) and (mp.ShippingStatus is null or (mp.ShippingStatus in ('InProgress','Shipped') and (rq.UniqueId is null or ISNULL(bs.IsSubmitted,0)=0)) or (mp.ShippingStatus = 'Error' and mp.ErrorDesc IS NULL) or (mp.ShippingStatus = 'PulledBack' and not exists (select Id from AM.PullBackRequests where MailPieceId = mp.Id)) or (mb.ClientId IS NULL))
 or (rq.MailPieceStatus='Shipped' and mp.IsPickedUp <> 1 and mp.DueDate < GETUTCDATE() and d.ShippingCarrier='FedEx' and mb.ReceivedOn>='2019-01-01 05:00:00')
END
