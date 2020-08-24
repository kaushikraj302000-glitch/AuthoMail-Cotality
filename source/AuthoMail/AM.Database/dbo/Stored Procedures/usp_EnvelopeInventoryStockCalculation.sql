
CREATE PROCEDURE [dbo].[usp_EnvelopeInventoryStockCalculation]
	
AS
BEGIN
	;with allenvelope as
(
	select EnvelopeId,cast(SubmittedOn as date) as SubmittedOn,Quantity,
ROW_NUMBER() OVER (PARTITION BY EnvelopeId order by cast(SubmittedOn as date) desc) as RowNum
	from PnS.EnvelopeUsage EU
	INNER JOIN PnS.PrintTrailBatches PTB ON EU.PrintTrailBatchId=PTB.Id
	where cast(SubmittedOn as date) < cast(GETUTCDATE() as date)
	GROUP BY cast(SubmittedOn as date),EnvelopeId,Quantity
),
monthAvg as
(
	select EnvelopeId,count(distinct(SubmittedOn)) as TotalDays,SUM(Quantity) AS TotalQuantityUsed
	from allenvelope
	where RowNum<=30
	GROUP BY EnvelopeId
),
weekAvg as
(
	select EnvelopeId,count(distinct(cast(SubmittedOn as date))) as TotalDays,SUM(Quantity) AS TotalQuantityUsed
	from allenvelope
	where RowNum<=7
	GROUP BY EnvelopeId
),
mergeAvg as
(
	select m.EnvelopeId,case when m.TotalQuantityUsed>isnull(w.TotalQuantityUsed,0) then m.TotalQuantityUsed else w.TotalQuantityUsed end as TotalQuantityUsed,
	case when m.TotalDays>isnull(w.TotalDays,0) then m.TotalDays else w.TotalDays end as TotalDays
	from monthAvg m
	left outer join weekAvg w on m.EnvelopeId=w.EnvelopeId
)
select c.*,es.CurrentQuantity,es.ReorderDuration,CEILING(cast(TotalQuantityUsed as float)/TotalDays) as AvgRate,CEILING(cast(es.CurrentQuantity as float)/(TotalQuantityUsed/TotalDays)) as EnvDays
from mergeAvg c
inner join Common.Envelopes e 
inner join Common.EnvelopeStock es ON es.EnvelopeId=e.Id
on c.EnvelopeId=e.Id
END
