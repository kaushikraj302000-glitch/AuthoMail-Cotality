-- =============================================
-- Author:		Sagar
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[ufn_UTCToEST]
(
	@UTCDate DATETIME
)
RETURNS DATETIME
AS
BEGIN
	

	-- Return the result of the function
--	RETURN CONVERT(DATETIME, SWITCHOFFSET(CONVERT(DATETIMEOFFSET, @UTCDate AT TIME ZONE 'UTC'), DATEPART(TZOFFSET, 
--SYSDATETIMEOFFSET() AT TIME ZONE 'Eastern Standard Time')))
RETURN CAST((CAST(@UTCDate AS datetime2) AT TIME ZONE 'UTC') AT TIME ZONE 'Eastern Standard Time' AS DATETIME)

END
