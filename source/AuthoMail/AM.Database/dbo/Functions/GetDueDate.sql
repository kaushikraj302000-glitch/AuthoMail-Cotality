
-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 30-07-2019
-- Description:	Calculate Due Date
-- =============================================
CREATE FUNCTION [dbo].[GetDueDate]
(
	@DocTypeId INT,
	@MailPieceId INT
)
RETURNS DATETIME
AS
BEGIN
	-- Declare the return variable here
	DECLARE @isSatWorkDay BIT,@isSunWorkDay BIT
	DECLARE @estEffectiveReceivedOn DATETIME, @estEffectiveReceivedOnDate DATE, @dayOfWeek VARCHAR(20)
	DECLARE @slaTimeSpan VARCHAR(20),@slaType VARCHAR(20)
	DECLARE @dueDate DATETIME,@timeSpan DATETIME,@noOfDays INT

	SET @isSatWorkDay=(SELECT CAST(CASE WHEN SatSLAType='NA' THEN 0 ELSE 1 END AS BIT) AS IsSatWorkDay FROM AM.DocTypes WHERE Id=@DocTypeId)

	SET @isSunWorkDay=(SELECT CAST(CASE WHEN SunSLAType='NA' THEN 0 ELSE 1 END AS BIT) AS IsSunWorkDay FROM AM.DocTypes WHERE Id=@DocTypeId)

	SET @estEffectiveReceivedOn=(SELECT EffectiveReceivedOnEst FROM AM.MailPieces WHERE Id=@MailPieceId)

	SET @estEffectiveReceivedOnDate=CAST(@estEffectiveReceivedOn AS DATE)

	SET @dayOfWeek=DATENAME(dw,@estEffectiveReceivedOnDate)

	SET @dueDate=@estEffectiveReceivedOn

	IF(@dayOfWeek='Saturday')
	BEGIN
		SET @slaTimeSpan = (SELECT SatTimeSpan FROM AM.DocTypes WHERE Id=@DocTypeId)
        SET @slaType = (SELECT SatSLAType FROM AM.DocTypes WHERE Id=@DocTypeId)
	END

	IF(@dayOfWeek='Sunday')
	BEGIN
		SET @slaTimeSpan = (SELECT SunTimeSpan FROM AM.DocTypes WHERE Id=@DocTypeId)
        SET @slaType = (SELECT SunSLAType FROM AM.DocTypes WHERE Id=@DocTypeId)
	END

	IF (@dayOfWeek <> 'Sunday' AND @dayOfWeek <> 'Saturday')
    BEGIN
		SET @slaTimeSpan = (SELECT BizdayTimeSpan FROM AM.DocTypes WHERE Id=@DocTypeId)
        SET @slaType = (SELECT BizdaySLAType FROM AM.DocTypes WHERE Id=@DocTypeId)
	END

	
	IF(@slaType='FixedTime')
	BEGIN
		SET @noOfDays=CAST(SUBSTRING(@slaTimeSpan,0,CHARINDEX(':',@slaTimeSpan,0)) AS INT)
		SET @timeSpan=CAST(SUBSTRING(@slaTimeSpan,CHARINDEX(':',@slaTimeSpan,0)+1,LEN(@slaTimeSpan)) AS DATETIME)
		SET @dueDate=DATEADD(DAY,@noOfDays,CAST(@estEffectiveReceivedOnDate AS DATETIME))+ @timeSpan

		DECLARE @cnt INT = 0;
		WHILE(@cnt < @noOfDays)
		BEGIN
			
			SET @dueDate=dbo.GetNextWorkingDay(@dueDate,@isSatWorkDay,@isSunWorkDay)
			SET @cnt = @cnt + 1;
		END
	END
	ELSE
	BEGIN
		SET @noOfDays=CAST(SUBSTRING(@slaTimeSpan,0,CHARINDEX(':',@slaTimeSpan,0)) AS INT)
		SET @timeSpan=CAST(SUBSTRING(@slaTimeSpan,CHARINDEX(':',@slaTimeSpan,0)+1,LEN(@slaTimeSpan)) AS DATETIME)
		SET @dueDate=DATEADD(DAY,@noOfDays,@estEffectiveReceivedOn)+ @timeSpan

		SET @cnt = 0;
		WHILE(@cnt < @noOfDays)
		BEGIN
			SET @DueDate=DATEADD(DAY,1,@DueDate)
			SET @dueDate=dbo.GetNextWorkingDay(@dueDate,@isSatWorkDay,@isSunWorkDay)
			SET @cnt = @cnt + 1;
		END
	END
	
	-- Return the result of the function
	RETURN [dbo].[ufn_ESTToUTC](@dueDate)

END
