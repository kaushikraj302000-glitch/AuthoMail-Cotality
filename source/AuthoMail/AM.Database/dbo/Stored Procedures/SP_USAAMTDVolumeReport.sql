CREATE PROCEDURE [dbo].[SP_USAAMTDVolumeReport]
	@startDate datetime,
	@endDate datetime
AS
	select isnull(m.DocumentOrPackageType,'') as DocumentType, isnull(m.ShippingStatus,'') as Status,isnull(ch.Title,'') as Channel, CONVERT(datetime, SWITCHOFFSET(b.ReceivedOn, DATEPART(TZOFFSET, 
b.ReceivedOn AT TIME ZONE 'Eastern Standard Time'))) as [ReceivedOn (EST)]
from AM.MailPieces m
inner join AM.MailBatches b on m.MailBatchId=b.Id
inner join Common.SFTChannels ch on b.SFTChannelId=ch.Id
where m.ClientId = (select Id from Common.Clients where Name='USAA') and m.ShippingStatus='Shipped'
and b.ReceivedOn between @startDate and @endDate
