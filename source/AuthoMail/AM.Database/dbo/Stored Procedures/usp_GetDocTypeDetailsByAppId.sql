

 CREATE PROCEDURE [dbo].[usp_GetDocTypeDetailsByAppId]
  @clientId int
 AS
 BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements.
  SET NOCOUNT ON;
  
  SELECT D.ClientId,D.[PrintSize],D.[ContentScalePercent],D.[QCPercent],D.[ClientEmails],D.[SupportEmails]
  ,D.[Description] as DocTypeDesc,D.DocTypeCode,D.IsActive,D.ModifiedOn,D.PrintFormat
  ,D.CreatedOn,D.Id AS DocTypeId,D.ShippingCarrier,D.ShippingServiceType,D.IsBRE
  FROM AM.DocTypes D
  INNER JOIN Common.Clients C ON C.Id=D.ClientId
  WHERE C.Id=@clientId
  
 END

