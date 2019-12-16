

CREATE PROCEDURE [dbo].[usp_GetSubmittedBatches]
@printVendorCode VARCHAR(50)
AS
BEGIN

IF(@printVendorCode='')
BEGIN
SELECT B.Id as PrintTrailBatchId,B.PrintBatchName
FROM [PnS].PrintTrailBatches B
INNER JOIN Common.PrintTrails PT ON B.PrintTrailId=PT.Id
INNER JOIN Common.PrintVendors PV ON PT.PrintVendorId=PV.Id
WHERE B.IsSubmitted=1 AND B.IsMRDFReceived=0
END
ELSE
BEGIN
SELECT B.Id as PrintTrailBatchId,B.PrintBatchName
FROM [PnS].PrintTrailBatches B
INNER JOIN Common.PrintTrails PT ON B.PrintTrailId=PT.Id
INNER JOIN Common.PrintVendors PV ON PT.PrintVendorId=PV.Id
WHERE B.IsSubmitted=1 AND B.IsMRDFReceived=0 AND PV.VendorCode=@printVendorCode
END

END

