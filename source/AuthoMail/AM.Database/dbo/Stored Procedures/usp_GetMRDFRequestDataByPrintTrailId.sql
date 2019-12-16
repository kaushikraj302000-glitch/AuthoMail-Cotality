

CREATE PROCEDURE [dbo].[usp_GetMRDFRequestDataByPrintTrailId] 
@printTrailId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT REQ.*,MP.Id as MailPieceId,ISNULL(MP.EffectiveReceivedOnEst,GETUTCDATE()) as EffectiveReceivedOnEst, BS.ClientId,BS.DocTypeId,D.ShippingCarrier,
 D.ShippingServiceType,C.AppCode,ISNULL(MP.DUEDATE,GETUTCDATE()) as MailPieceDueDate,D.EnvelopeId,D.BREEnvelopeId
 FROM PnS.MRDFRequest REQ
 INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
 LEFT OUTER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
 INNER JOIN AM.DocTypes D ON BS.DocTypeId=D.Id
 INNER JOIN Common.Clients C ON C.Id=BS.ClientId
 WHERE BS.PipelineStatus='Prepped' AND  IsPrepped = 1 AND IsSubmitted = 0
 AND BS.PrintTrailId=@printTrailId
	
END

