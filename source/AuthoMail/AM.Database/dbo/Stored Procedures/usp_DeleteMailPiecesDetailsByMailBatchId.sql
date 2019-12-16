



CREATE PROCEDURE [dbo].[usp_DeleteMailPiecesDetailsByMailBatchId]
@mailBatchId int
AS
BEGIN
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;

DELETE FROM [AM].[PrintBatches] WHERE MailBatchId=@mailBatchId	
DELETE FROM [AM].[MailPieceCustomInfo] WHERE MailPieceId IN (SELECT Id FROM [AM].[MailPieces] WHERE MailBatchId=@mailBatchId)
DELETE FROM [AM].[MailPieces] WHERE MailBatchId=@mailBatchId 

END

