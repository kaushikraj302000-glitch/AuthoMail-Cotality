
CREATE PROCEDURE [dbo].[usp_DeleteMRDFRequestByJobId]
	@jobId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DELETE FROM PnS.MRDFRequest WHERE JobId=@jobId
END



























