CREATE PROCEDURE [dbo].[usp_GetMRDFRequestDataByPrintTrailId] 
@printTrailId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets FROM
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT REQ.*,MP.Id AS MailPieceId,MP.EffectiveReceivedOnEst, BS.ClientId,BS.DocTypeId,D.ShippingCarrier,
 D.ShippingServiceType,C.AppCode,MP.DueDate AS MailPieceDueDate,D.EnvelopeId,D.BREEnvelopeId,E.EnvelopeName,BE.EnvelopeName AS BREEnvelopeName
  ,E.PartNumber AS EnvelopePartNo,BE.PartNumber AS BREPartNo,E.EnvelopeName AS EnvelopeDesc,BE.EnvelopeName AS BREDesc
 FROM PnS.MRDFRequest REQ
 INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
 LEFT OUTER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
 INNER JOIN [dbo].[v_DocTypes] d ON MP.DocTypeId=d.Id AND mp.PnSConfigDetailId = d.PnSConfigDetailId
 INNER JOIN Common.Clients C ON C.Id=BS.ClientId
 LEFT OUTER JOIN Common.Envelopes E ON D.EnvelopeId=E.Id
 LEFT OUTER JOIN Common.Envelopes BE ON D.BREEnvelopeId=BE.Id
 WHERE BS.PipelineStatus='Prepped' AND  IsPrepped = 1 AND IsSubmitted = 0 
 AND BS.PrintTrailId=@printTrailId
	
END
