-- =============================================
-- Author:		Milan Patel
-- Create date: 03-Mar-18
-- Description:	Get MailPiece by batchid.
-- =============================================
CREATE PROCEDURE [dbo].[uspweb_GetMailPieceByBatchId]
	-- Add the parameters for the stored procedure here
	@batchId int,
	@clientId int = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	select *
from [AM].[MailBatches] b,[Common].[Clients] c,[AM].[MailPieces] m
left outer join [AM].[DocTypes] d ON m.DocTypeId=d.Id
where m.MailBatchId=b.Id and b.ClientId=ISNULL(@clientId,b.ClientId) and m.ClientId=c.Id
and b.Id=@batchId
END
