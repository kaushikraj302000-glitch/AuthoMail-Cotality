


			CREATE PROCEDURE [dbo].[usp_GetMRDFReceivedBatches]
			AS
			BEGIN

			SELECT B.PrintBatch,DT.DocTypeCode,C.AppCode,BU.CustCode,CONVERT(VARCHAR(10),B.PrintBatch) as BatchCode 
			FROM [PnS].BatchSummary B
			INNER JOIN [AM].DocTypes DT ON DT.Id=B.DocTypeId 
			INNER JOIN [Common].Clients C ON C.Id=B.ClientId
			INNER JOIN [Common].BusinessUnit BU ON BU.Id=C.BusinessUnit
			WHERE B.IsMRDFReceived=1 AND B.IsPDFsReceived=0 AND B.SubmittedOn > '2018-08-01' AND B.PrintLocation='Fort Mill, SC'
			END

