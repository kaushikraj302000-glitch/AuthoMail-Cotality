
CREATE VIEW [dbo].[view_FedExData]
AS
SELECT        MP.Id, MP.UniqueId, C.AppCode, MB.PipelineStatus, MP.ShippingStatus, D.DocTypeCode, D.ShippingCarrier, D.ShippingServiceType, MP.ClientId, MP.PrintLocation, 
                         MB.BatchName, MP.BusinessFriendlyIdValue AS LoanNumber, MP.RcptFirstName, MP.RcptLastName, MP.RcptAddressStreet, MP.RcptAddressAptOrSuiteOrOther, 
                         MP.RcptAddressCity, MP.RcptAddressState, MP.RcptAddressZIP, MP.TrackingNo, MB.ReceivedOn, MP.DocumentOrPackageType, MP.IsPickedUp, MP.PickedUpOn, 
                         MP.DueDate, MP.ShippedOn, MP.EffectiveReceivedOnEst
FROM            AM.MailPieces AS MP INNER JOIN
                         AM.MailBatches AS MB ON MB.Id = MP.MailBatchId INNER JOIN
                         AM.DocTypes AS D ON D.Id = MP.DocTypeId INNER JOIN
                         Common.Clients AS C ON MB.ClientId = C.Id
WHERE        (MP.ShippingStatus IN ('InProgress', 'Shipped')) AND (D.ShippingCarrier = 'FedEx') AND (MB.ReceivedOn >= '2019-01-01 05:00:00')

