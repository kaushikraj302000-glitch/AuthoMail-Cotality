
CREATE PROCEDURE [dbo].[usp_EnvelopeInventoryStockCalculation]
	
AS
BEGIN
	;with allenvelope as
(
	select EnvelopeId,ShipperId,cast(SubmittedOn as date) as SubmittedOn,Quantity,
ROW_NUMBER() OVER (PARTITION BY EnvelopeId order by cast(SubmittedOn as date) desc) as RowNum
	from PnS.EnvelopeUsage EU
	INNER JOIN PnS.PrintTrailBatches PTB ON EU.PrintTrailBatchId=PTB.Id
	where cast(SubmittedOn as date) < cast(GETUTCDATE() as date)
	GROUP BY cast(SubmittedOn as date),EnvelopeId,Quantity,ShipperId
),
monthAvg as
(
	select EnvelopeId,ShipperId,count(distinct(SubmittedOn)) as TotalDays,SUM(Quantity) AS TotalQuantityUsed
	from allenvelope
	where RowNum<=30
	GROUP BY EnvelopeId,ShipperId
),
weekAvg as
(
	select EnvelopeId,ShipperId,count(distinct(cast(SubmittedOn as date))) as TotalDays,SUM(Quantity) AS TotalQuantityUsed
	from allenvelope
	where RowNum<=7
	GROUP BY EnvelopeId,ShipperId
),
mergeAvg as
(
	select m.EnvelopeId,m.ShipperId,case when m.TotalQuantityUsed>isnull(w.TotalQuantityUsed,0) then m.TotalQuantityUsed else w.TotalQuantityUsed end as TotalQuantityUsed,
	case when m.TotalDays>isnull(w.TotalDays,0) then m.TotalDays else w.TotalDays end as TotalDays
	from monthAvg m
	left outer join weekAvg w on m.EnvelopeId=w.EnvelopeId
)
select c.*,e.EnvelopeName,Case When SI.PrintLocation is null Then NULL Else 
CONCAT(SI.PrintLocation, CONCAT(' (', SI.PrintVendorCode, ') ')) End as ShipperName ,es.CurrentQuantity,es.ReorderDuration,CEILING(cast(TotalQuantityUsed as float)/TotalDays) as AvgRate,CEILING(cast(es.CurrentQuantity as float)/(TotalQuantityUsed/TotalDays)) as EnvDays
from mergeAvg c
inner join Common.Envelopes e on c.EnvelopeId=e.Id
inner join Common.EnvelopeStock  es on e.Id=es.EnvelopeId and c.ShipperId=es.ShipperId
inner JOIN Common.ShipperInformation SI ON c.ShipperId=SI.Id
END