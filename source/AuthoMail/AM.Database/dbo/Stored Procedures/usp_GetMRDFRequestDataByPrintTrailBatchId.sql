CREATE PROCEDURE [dbo].[usp_GetMRDFRequestDataByPrintTrailBatchId]
@printTrailBatchId INT,
@mailPieceStatus VARCHAR(255)
AS
BEGIN
 -- SET NOCOUNT ON added to prevent extra result sets FROM
 -- interfering with SELECT statements.
 SET NOCOUNT ON;
 IF(@mailPieceStatus='ALL')
 BEGIN
  SELECT Req.JobId, Req.PieceId, Req.[Date], Req.TotalSheets, Req.InputWeight, Req.Feeder3_Sel, Req.Feeder4_Sel
  , Req.Feeder5_Sel, Req.LowCap_Sel, Req.NoSeal_Sel, Req.Feeder2_Sel, Req.ZipMark_Sel, Req.Address1, Req.Address2
  , Req.Zip5, Req.Zip4, Req.Zip2, Req.DivertVS2, Req.DivertVS3
  , Req.DivertVS4, Req.ReturnLogoBitMap, Req.ReturnLine1, Req.ReturnLine2, Req.ReturnLine3, Req.ReturnLine4
  , Req.ReturnLine5, Req.ReturnLine6, Req.GroupNumber, Req.SocialSecNumber, Req.CheckAmount, Req.PassThru
  , Req.Expansion, Req.EndofRecord, Req.UniqueId, Req.PDFFileName,ErrId, Req.ErrDesc, Req.MailPieceStatus
  , Req.IsPDFReceived,PrintTrailBatchId,MP.Id AS MailPieceId
  FROM [PnS].[MRDFRequest] Req
  INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
  LEFT OUTER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
  WHERE Req.PrintTrailBatchId = @printTrailBatchId
 END
 ELSE IF(@mailPieceStatus='INTERNAL')--This will be used for getting only submitted records to PCI
 BEGIN
  SELECT JobId, PieceId, [Date], TotalSheets, InputWeight, Feeder3_Sel, Feeder4_Sel
  , Feeder5_Sel, LowCap_Sel, NoSeal_Sel, Feeder2_Sel, ZipMark_Sel, Address1, Address2
  , Zip5, Zip4, Zip2, DivertVS2, DivertVS3
  , DivertVS4, ReturnLogoBitMap, ReturnLine1, ReturnLine2, ReturnLine3, ReturnLine4
  , ReturnLine5, ReturnLine6, GroupNumber, SocialSecNumber, CheckAmount, PassThru
  , Expansion, EndofRecord, UniqueId, PDFFileName,ErrId, ErrDesc, MailPieceStatus, IsPDFReceived,PrintTrailBatchId 
  FROM [PnS].[MRDFRequest] WHERE PrintTrailBatchId = @printTrailBatchId AND ErrId NOT IN (-1,-2,-3)
 END
 ELSE
 BEGIN
   SELECT REQ.*,MP.Id AS MailPieceId,MP.EffectiveReceivedOnEst, BS.ClientId,BS.DocTypeId,PCD.ShippingCarrier,
  PCD.ShippingServiceType,C.AppCode,CASE WHEN C.Alias IS NULL OR C.Alias='' THEN C.AppCode ELSE C.Alias END AS Alias,MP.DueDate AS MailPieceDueDate,PCD.EnvelopeId,d.BREEnvelopeId,E.EnvelopeName,BE.EnvelopeName AS BREEnvelopeName
  ,E.PartNumber AS EnvelopePartNo,BE.PartNumber AS BREPartNo,E.EnvelopeName AS EnvelopeDesc,BE.EnvelopeName AS BREDesc,PCD.PrintFormat
  ,PCD.PrintSize,E.EnvelopeDesc AS PackageCode,(SELECT [Value] FROM AM.MailPieceCustomInfo MPC WHERE MPC.[Key]='CustomerId' AND MPC.MailPieceId=MP.Id) AS CustomerId
  ,CAST((CASE WHEN LEN(LTRIM(RTRIM(MP.RcptAddressCountry))) >0 THEN 1 ELSE 0 END )AS BIT) AS IsForeignAddress,MP.ShipperId, MP.Inserts
  ,InsertsPartNos=STUFF
  (  
    (  
      SELECT DISTINCT '|'+ CAST([PartNumber] AS VARCHAR(MAX))  
      FROM Common.Envelopes   
      WHERE UPPER(EnvelopeName) IN (SELECT  UPPER(RTRIM(LTRIM(value))) FROM string_split((MP.Inserts),',') WHERE RTRIM(LTRIM(value)) <>'')   
      FOR XMl PATH('')  
    ),1,1,'')
  FROM PnS.MRDFRequest REQ
  INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
  LEFT OUTER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
  INNER JOIN [AM].[DocTypes] d ON mp.DocTypeId=d.Id
  INNER JOIN Common.PnSConfigDetail PCD ON MP.PnSConfigDetailId = pcd.Id
  INNER JOIN Common.Clients C ON C.Id=BS.ClientId
  LEFT OUTER JOIN Common.Envelopes E ON PCD.EnvelopeId=E.Id
  LEFT OUTER JOIN Common.Envelopes BE ON D.BREEnvelopeId=BE.Id
  WHERE REQ.PrintTrailBatchId=@printTrailBatchId AND ISNULL(MailPieceStatus,'')=@mailPieceStatus 
 END
END

