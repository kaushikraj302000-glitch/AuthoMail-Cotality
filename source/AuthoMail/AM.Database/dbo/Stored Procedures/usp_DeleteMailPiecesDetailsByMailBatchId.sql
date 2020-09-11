CREATE PROCEDURE [dbo].[usp_DeleteMailPiecesDetailsByMailBatchId]
@mailBatchId int
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM [AM].[PrintBatches] WHERE MailBatchId=@mailBatchId)
BEGIN
	DELETE FROM [AM].[PrintBatches] WHERE MailBatchId=@mailBatchId	
END

IF EXISTS (SELECT 1 FROM [AM].[MailPieceCustomInfo] WHERE MailPieceId IN (SELECT Id FROM [AM].[MailPieces] WHERE MailBatchId=@mailBatchId))
BEGIN
	DELETE FROM [AM].[MailPieceCustomInfo] WHERE MailPieceId IN (SELECT Id FROM [AM].[MailPieces] WHERE MailBatchId=@mailBatchId)
END

IF EXISTS (SELECT 1 FROM [AM].[EDeliveryMailPieces] WHERE MailBatchId=@mailBatchId)
BEGIN
	DELETE FROM [AM].[EDeliveryMailPieces] WHERE MailBatchId=@mailBatchId
END

IF EXISTS (SELECT 1 FROM [AM].[MailPieces] WHERE MailBatchId=@mailBatchId)
BEGIN
	DELETE FROM [AM].[MailPieces] WHERE MailBatchId=@mailBatchId 
END

END

