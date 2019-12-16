CREATE PROCEDURE [dbo].[SP_GetScanStatsOld]
	@appcode varchar(128),
	@EstDate DATE,
	@startdate datetime,
	@enddate datetime
 AS
BEGIN
	declare @printlocationcl varchar(1024), @totalPending int, @scannedCount int, @clientid int

	-- Set PrintLocation value
	select @printlocationcl=Name from Common.PrintVendors where VendorCode='CL_DALLAS';
	--select @printlocationcl=Name from Common.PrintVendors where VendorCode='CL_DALLAS';
	--select @printlocationcl = Value from [Common].[SystemSettings] where Id ='Common_PrintLocationCL'

	-- Remove AUT from app code
	set @appcode = replace(@appcode, 'AUT','')

	-- Get client id
	select @clientid=Id from Common.Clients where AppCode=@appcode

	-- Calculate Total Pending Scanned
		select @totalPending=count(Id) from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where Printlocation=@printlocationcl and ClientId=@clientId
		and MailPieceStatus = 'InProgress' and IsSubmitted=1
		

	-- Calculate Total Scanned count
		select @scannedCount=count(Id) from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where Printlocation=@printlocationcl and ClientId=@clientId
		and MailPieceStatus <> 'InProgress' and (ProcessingTimeStamp between @startdate and @enddate)
	
		-- Return result
	select @scannedCount as [ScanCount],@totalPending + @scannedCount as [TotalCount]

END

