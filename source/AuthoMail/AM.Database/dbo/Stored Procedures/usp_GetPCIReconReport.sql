-- =============================================
	-- Author:		Sagar Golvelkar
	-- Create date: 09/12/2018
	-- Description:	<Description,,>
	-- =============================================
	CREATE PROCEDURE [dbo].[usp_GetPCIReconReport]
	@StartDate DATE,
	@EndDate DATE
	AS
	BEGIN
		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;
		SELECT * FROM 
		(
			--USAA,ALASKA
			SELECT DISTINCT B.PrintBatch,B.PrintFileName,B.SubmittedOn,B.DueDate,D.ShippingCarrier,D.ShippingServiceType,COUNT(*) AS PackageCount,SUM(M.TotalPages) as TotalPages FROM PnS.BatchSummary B INNER JOIN PnS.MRDFRequest REQ ON REQ.JobId=B.PrintBatch INNER JOIN AM.MailPieces M ON M.UniqueId=REQ.UniqueId INNER JOIN AM.DocTypes D ON D.Id=B.DocTypeId WHERE B.PrintLocation='Fort Mill, SC' AND B.ClientId IN (101,102) AND M.EffectiveReceivedOnEst >= @StartDate AND M.EffectiveReceivedOnEst < @EndDate GROUP BY B.PrintBatch,B.PrintFileName,B.SubmittedOn,B.DueDate,D.ShippingCarrier,D.ShippingServiceType,B.Images
			UNION
			--HSBC
			SELECT DISTINCT B.PrintBatch,B.PrintFileName,B.SubmittedOn,B.DueDate,D.ShippingCarrier,D.ShippingServiceType,COUNT(*) AS PackageCount,SUM(M.TotalPages) as TotalPages FROM PnS.BatchSummary B INNER JOIN PnS.MRDFRequest REQ ON REQ.JobId=B.PrintBatch INNER JOIN AM.MailPieces M ON M.UniqueId=REQ.UniqueId INNER JOIN AM.DocTypes D ON D.Id=B.DocTypeId WHERE B.PrintLocation='Fort Mill, SC' AND B.ClientId IN (104) AND CAST(dbo.ufn_UTCToEST(B.SubmittedOn) as DATE) = @StartDate GROUP BY B.PrintBatch,B.PrintFileName,B.SubmittedOn,B.DueDate,D.ShippingCarrier,D.ShippingServiceType,B.Images
			UNION
			--CONDOSAFE,RPS,CITI,DALLAS
			SELECT DISTINCT B.PrintBatch,B.PrintFileName,B.SubmittedOn,DATEADD(Day,1,B.DueDate) as DueDate,D.ShippingCarrier,D.ShippingServiceType,COUNT(*) AS PackageCount,B.Images as TotalPages FROM PnS.BatchSummary B INNER JOIN PnS.MRDFRequest REQ ON REQ.JobId=B.PrintBatch
			INNER JOIN AM.DocTypes D ON D.Id=B.DocTypeId
			WHERE B.ClientId IN (103,107,109,110) AND CAST(dbo.ufn_UTCToEST(B.SubmittedOn) as DATE) = @StartDate
			GROUP BY B.PrintBatch,B.PrintFileName,B.SubmittedOn,B.DueDate,D.ShippingCarrier,D.ShippingServiceType,B.Images
		)X
		ORDER BY X.PRINTBATCH
	END
