
-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 30-07-2019
-- Description:	Get Print Trail Id
-- =============================================
CREATE FUNCTION [dbo].[GetPrintTrail] 
(
	@DocTypeId INT,
	@MailPieceId INT
)
RETURNS INT
AS
BEGIN
	-- Declare the return variable here
	DECLARE @EffectiveReceivedOnEst DATETIME,@dayofWeek VARCHAR(20),@PrintTrailId INT,@TimeSpan TIME(7)

	SET @EffectiveReceivedOnEst=(SELECT EffectiveReceivedOnEst FROM AM.MailPieces WHERE Id=@MailPieceId)
	
	SET @TimeSpan=CAST(@EffectiveReceivedOnEst AS TIME(0))

	SET @dayOfWeek=DATENAME(dw,@EffectiveReceivedOnEst)

	IF (@dayofWeek = 'Saturday')
	BEGIN
		SET @PrintTrailId = (SELECT TOP 1 PrintTrailId FROM Common.DocTypePrintTrailMaps WHERE DocTypeId=@DocTypeId AND @TimeSpan BETWEEN SatStartTime AND SatEndTime)
	END
	ELSE IF(@dayofWeek = 'Sunday')
	BEGIN
		SET @PrintTrailId = (SELECT TOP 1 PrintTrailId FROM Common.DocTypePrintTrailMaps WHERE DocTypeId=@DocTypeId AND @TimeSpan BETWEEN SunStartTime AND SunEndTime)
	END
	ELSE
	BEGIN
		SET @PrintTrailId = (SELECT TOP 1 PrintTrailId FROM Common.DocTypePrintTrailMaps WHERE DocTypeId=@DocTypeId AND @TimeSpan BETWEEN BizDayStartTime AND BizDayEndTime)
	END

	-- Return the result of the function
	RETURN @PrintTrailId

END
