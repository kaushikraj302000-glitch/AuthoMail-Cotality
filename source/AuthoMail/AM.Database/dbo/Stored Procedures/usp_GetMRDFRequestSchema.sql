
CREATE PROCEDURE [dbo].[usp_GetMRDFRequestSchema]
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	SELECT [Id]
      ,[Fieldname]
      ,[Type]
      ,[Startposition]
      ,[Length]
      ,[IsActive]
      ,[Description]
      ,[Comments]
      ,[SampleValue]
      ,[DataType]
      ,[DataFormatTemplate]
      ,[DefaultValue]
  FROM [PnS].[Schema_MRDFRequest]	WHERE IsActive = 1
  ORDER BY Id
END


























