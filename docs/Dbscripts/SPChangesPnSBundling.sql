/****** Object:  StoredProcedure [dbo].[usp_GetBatchSummaryById]    Script Date: 11/1/2018 6:40:27 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[usp_GetBatchSummaryById]
	-- Add the parameters for the stored procedure here
	@batchId int
AS
BEGIN

	SELECT PrintBatch, ClientId, JobName, PrintFileName, BatchIdentifier, PipelineStatus, ValidationStatus, OnValidationErrors,
 ValidationErrors, BatchStatus, IsReceived, ReceivedOn, IsDownloaded, DownloadedOn, IsPrepped, PreppedOn, IsSubmitted,
 SubmittedOn, IsMRDFReceived, MRDFReceivedOn, IsReportPrepped, ReportPreppedOn, IsReportedBack, ReportedBackOn, IsSuspended, 
 SuspendedOn, IsCancelled, CancelledOn, JulianDate, Images, DocTypeId,SLADays,DueDate,PrintLocation,PrintTrailId
  FROM [PnS].[BatchSummary] WHERE PrintBatch = @batchId
END

/****** Object:  StoredProcedure [dbo].[usp_GetAllClientAppDetail]    Script Date: 11/16/2017 17:20:05 ******/
SET ANSI_NULLS ON


/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestDataByPrintTrailId]    Script Date: 11/1/2018 6:40:56 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 10/31/2018
-- Description:	Get all the mrdf request details by print trails id
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetMRDFRequestDataByPrintTrailId] 
@printTrailId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT REQ.*,MP.Id as MailPieceId,MP.EffectiveReceivedOnEst, BS.ClientId,MP.DocTypeId,D.ShippingCarrier,
	D.ShippingServiceType,C.AppCode
	FROM PnS.MRDFRequest REQ
	INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
	INNER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
	INNER JOIN AM.DocTypes D ON MP.DocTypeId=D.Id
	INNER JOIN Common.Clients C ON C.Id=MP.ClientId
	WHERE BS.PipelineStatus='Prepped' AND  IsPrepped = 1 AND IsSubmitted = 0 AND REQ.MailPieceStatus='InProgress'
	AND BS.PrintTrailId=@printTrailId
	
END

GO


/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseDataByPrintTrailBatchId]    Script Date: 11/1/2018 6:42:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[usp_GetMRDFResponseDataByPrintTrailBatchId]
@jobId int
AS
BEGIN
 -- SET NOCOUNT ON added to prevent extra result sets from
 -- interfering with SELECT statements.
 SET NOCOUNT ON;
 
 SELECT JobId, PieceId, Date, Feeder2_Fed, OperatorId, MachineId, ProcessingTimeStamp, DetailedErrorId
 , Disposition, Input_Fed, Feeder3_Fed, Feeder4_Fed, Feeder5_Fed, ComputedWeight, MeterDate
 , StatusSource, PostageMetered, WeightMeasured, ExitLocation, TotalSheets, PDFFileName, PackageTrackingNumber 
 FROM [Pns].[MRDFResponse] MS 
 WHERE MS.JobId=@jobId
END

/****** Object:  StoredProcedure [dbo].[usp_GetSubmittedBatches]    Script Date: 11/1/2018 6:43:01 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[usp_GetSubmittedBatches]
AS
BEGIN

--SELECT * FROM dbo.BatchSummary
SELECT B.Id as PrintTrailBatchId,B.PrintBatchName
FROM [PnS].PrintTrailBatches B
INNER JOIN Common.PrintTrails PT ON B.PrintTrailId=PT.Id
INNER JOIN Common.PrintVendors PV ON PT.PrintVendorId=PV.Id
WHERE B.IsSubmitted=1 AND B.IsMRDFReceived=0 

END


/****** Object:  StoredProcedure [dbo].[usp_GetPrintBatchDetailByPrintBatchTrailId]    Script Date: 11/1/2018 6:43:53 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 10/31/2018
-- Description:	Get PrintTrailBatch details
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetPrintBatchDetailByPrintBatchTrailId]
@printTrailBatchId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   SELECT * 
   FROM PnS.PrintTrailBatches
   WHERE Id=@printTrailBatchId
END

GO

/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseDataByPrintTrailBatchId]    Script Date: 11/1/2018 6:44:19 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[usp_GetMRDFResponseDataByPrintTrailBatchId]
@jobId int
AS
BEGIN
 -- SET NOCOUNT ON added to prevent extra result sets from
 -- interfering with SELECT statements.
 SET NOCOUNT ON;
 
 SELECT JobId, PieceId, Date, Feeder2_Fed, OperatorId, MachineId, ProcessingTimeStamp, DetailedErrorId
 , Disposition, Input_Fed, Feeder3_Fed, Feeder4_Fed, Feeder5_Fed, ComputedWeight, MeterDate
 , StatusSource, PostageMetered, WeightMeasured, ExitLocation, TotalSheets, PDFFileName, PackageTrackingNumber 
 FROM [Pns].[MRDFResponse] MS 
 WHERE MS.JobId=@jobId
END


/****** Object:  StoredProcedure [dbo].[usp_GetPrintTrailDetailByPrintTrailId]    Script Date: 11/1/2018 6:45:37 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 10/31/2018
-- Description:	Get Print Trail details by 
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetPrintTrailDetailByPrintTrailId] 
@printTrailId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT TOP 1 PT.id as PrintTrailId, PT.PrintVendorId,PT.TrailName,
	PV.Name as PrintVendorName,PV.BusinessCode ,PV.SFTChannelId,
	SC.Protocol,SC.Host,SC.UserName,SC.[Password],SC.ToADEDir,SC.FromADEDir 
	FROM Common.PrintTrails PT
	INNER JOIN Common.PrintVendors PV ON PV.Id=PT.PrintVendorId
	INNER JOIN Common.SFTChannels SC ON PV.SFTChannelId=SC.Id
	WHERE PT.Id=@printTrailId
END

GO

/****** Object:  StoredProcedure [dbo].[usp_GetPrintTrailDetailByPrintTrailName]    Script Date: 11/1/2018 6:46:04 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 10/31/2018
-- Description:	Get Print Trail details by 
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetPrintTrailDetailByPrintTrailName] 
@printTrailName VARCHAR(255)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT TOP 1 PT.id as PrintTrailId, PT.PrintVendorId,PT.TrailName,
	PV.Name as PrintVendorName,PV.BusinessCode ,PV.SFTChannelId,
	SC.Protocol,SC.Host,SC.UserName,SC.[Password],SC.ToADEDir,SC.FromADEDir 
	FROM Common.PrintTrails PT
	INNER JOIN Common.PrintVendors PV ON PV.Id=PT.PrintVendorId
	INNER JOIN Common.SFTChannels SC ON PV.SFTChannelId=SC.Id
	WHERE PT.TrailName=@printTrailName
END

GO

