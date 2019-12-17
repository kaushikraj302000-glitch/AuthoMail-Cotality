

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
	RETURN DATEADD(HOUR,-5,@UTCDate)

END


