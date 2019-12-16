CREATE PROCEDURE [dbo].[SP_GetRecentBatchesStatsOld] 
	@appcode varchar(128),
	@EstDate DATE,
	@startdate datetime,
	@enddate datetime
 AS
BEGIN
	
	declare @bn nvarchar(max),@st time, @et time,
	@printlocationcl varchar(1024),  @clientid int,
	@sd datetime, @ed datetime, @mindt datetime=null

	--Get Client Id, and remove AUT from client appcode
	set @appcode=REPLACE(@appcode,'AUT','')
	select @clientid=Id from Common.Clients where AppCode=@appcode;
	-- Get PrintLocation
	select @printlocationcl=Name from Common.PrintVendors where VendorCode='CL_DALLAS';
	--select @printlocationcl=Value from [Common].[SystemSettings] where Id ='Common_PrintLocationCL';
	-- Get Pending count batch(sweep) wise
	with pc as
	(
		select distinct BatchName,count(Id) as PendingCount from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where Printlocation=@printlocationcl and ClientId=@clientId
		and MailPieceStatus = 'InProgress' and IsSubmitted=1
		group by BatchName
	),
	-- Get Scanned count batch(sweep) wise
	sc as
	(
		select distinct BatchName,count(Id) as ScannedCount from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where Printlocation=@printlocationcl and ClientId=@clientId and IsSubmitted=1
		and MailPieceStatus <> 'InProgress' and ProcessingTimeStamp between @startdate and @enddate
		group by BatchName
	),
	sw as
	(
		select distinct BatchName from dbo.ufn_GetDallasBatches(@EstDate) where AppCode=@appcode
	),

	t as
	(	
		select	ISNULL(sc.BatchName,pc.BatchName) as BatchName,
				ISNULL(pc.PendingCount,0) as PendingCount,
				ISNULL(sc.ScannedCount,0) as ScannedCount
		from sc full outer join pc on sc.BatchName=pc.BatchName
	)
	
	select	ISNULL(t.BatchName,sw.BatchName) as BatchName,
			IsNULL(t.ScannedCount,0) as ScannedLoans,
			(ISNULL(t.PendingCount,0) + ISNULL(t.ScannedCount,0)) as TotalLoans 
			from t
			full outer join sw on t.BatchName = sw.BatchName
			order by BatchName
END
