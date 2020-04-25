CREATE  PROCEDURE [dbo].[usp_GetMRDFRequestDataByPrintTrailId] 
@printTrailId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets FROM
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

		SELECT REQ.*,MP.Id AS MailPieceId,MP.EffectiveReceivedOnEst, BS.ClientId,BS.DocTypeId,PCD.ShippingCarrier,
 PCD.ShippingServiceType,C.AppCode,CASE WHEN C.Alias IS NULL OR C.Alias='' THEN C.AppCode ELSE C.Alias END AS Alias,MP.DueDate AS MailPieceDueDate,PCD.EnvelopeId,D.BREEnvelopeId,E.EnvelopeName,BE.EnvelopeName AS BREEnvelopeName
  ,E.PartNumber AS EnvelopePartNo,BE.PartNumber AS BREPartNo,E.EnvelopeName AS EnvelopeDesc,BE.EnvelopeName AS BREDesc
 FROM PnS.MRDFRequest REQ
 INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
 LEFT OUTER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
 INNER JOIN [AM].[DocTypes] d ON mp.DocTypeId=d.Id
  INNER JOIN Common.PnSConfigDetail PCD ON MP.PnSConfigDetailId = pcd.Id
 INNER JOIN Common.Clients C ON C.Id=BS.ClientId
 LEFT OUTER JOIN Common.Envelopes E ON PCD.EnvelopeId=E.Id
 LEFT OUTER JOIN Common.Envelopes BE ON D.BREEnvelopeId=BE.Id
 WHERE BS.PipelineStatus='Prepped' AND  IsPrepped = 1 AND IsSubmitted = 0 AND REQ.PrintTrailBatchId IS NULL
 AND BS.PrintTrailId=@printTrailId
	
END
