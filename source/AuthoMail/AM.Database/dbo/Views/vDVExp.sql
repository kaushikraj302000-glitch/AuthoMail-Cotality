


CREATE view [dbo].[vDVExp]
as
select
m.Id as PkgId
,MailBatchId
,m.UniqueId as PkgUniqueId
,m.BusinessFriendlyIdValue as BFId
,c.AppCode as ClientCode
,sftc.Code as ChannelCode
,DocTypeCode
,m.TrackingNo
,PrintFormat
,PrintSize
,EnvelopeId as EnvPartNo
,IsBRE
,BREEnvelopeId as BREEnvPartNo
,ShippingCarrier
,ShippingServiceType
,ShippingSpecialService
,BizdayCutOff
,SatCutOff
,QCPercent
,ShippingStatus
,TotalPages
,m.TotalSheets
,mrq.TotalSheets as PnSTotalSheets
,ErrorCode as ErrCode
,IsPullBackRequested
,IsPulledBack
,IsQCPerformed
,IsQCRequested
,m.PrintLocation
,TrailName
,RcptAddressCity
,RcptAddressState
,RcptAddressZIP
,RcptAddressTypes
,b.BatchStatus
,dbo.ufn_UTCToEST(b.ReceivedOn) as ReceivedOnEst
,EffectiveReceivedOnEst
,dbo.ufn_UTCToEST(m.DueDate) as DueDateEst
,dbo.ufn_UTCToEST(bs.SubmittedOn) as SubmittedOnEst
,dbo.ufn_UTCToEST(ShippedOn) as ShippedOnEst
,dbo.ufn_UTCToEST(b.ReportedBackOn) as ReportedBackOnEst
,m.SLADays as SLABuffer
,MissedSLAReason

from AM.MailPieces m
inner join AM.MailBatches b on m.MailBatchId = b.Id
inner join Common.Clients c on b.ClientId = c.Id
inner join Common.SFTChannels sftc on b.SFTChannelId = sftc.Id
left outer join AM.DocTypes d on m.DocTypeId = d.Id
left outer join Common.PrintTrails pt on m.PrintTrailId = pt.Id
left outer join PnS.MRDFRequest mrq on m.UniqueId = mrq.UniqueId
left outer join PnS.BatchSummary bs on mrq.JobId = bs.PrintBatch

