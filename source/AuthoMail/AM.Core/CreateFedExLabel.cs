using AM.Core.ShipWebServiceRef;
using AM.Entities;
using AM.Services;
using AM.Lib;
using iTextSharp.text;
using iTextSharp.text.pdf;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AM.Core
{
    class CreateFedExLabel
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string GeneratedFedExLabelPath { get; private set; }

        public bool Create(MailPieceDTO mailPiece, FedExAccountInfo fedExAccInfo, DocType docType, MailBatchInfo mbi, DateTime fedExShipTimeStamp)
        {
            //Here we are assigning No_Error because if we have any general failure or end point exception then we are trying again.
            mailPiece.ErrorCode = OutComeCodes.No_Error;
            mailPiece.ErrorDesc = null;
            mailPiece.ShippingStatus = ShippingStatus.InProgress.ToString();

            bool isLabelGenerated = false;

            ProcessShipmentRequest request = CreateShipmentRequest(mailPiece, fedExAccInfo, docType, fedExShipTimeStamp);
            // Initialize the service
            ShipPortTypeClient service = new ShipPortTypeClient();

            // For production account we always need to use production URL. Devang sir clarified that until the shipping label barcode is scanned by FedEx we will not get billed. So, if needed for UAT also we can use production URL.
            if (Config.Common_Environment == Env.Prod)
            {
                service.Endpoint.Address = new System.ServiceModel.EndpointAddress(Config.Core_FedExProdService);
            }
            else
            {
                service.Endpoint.Address = new System.ServiceModel.EndpointAddress(Config.Core_FedExTestService);
            }

            try
            {
                // Call the ship web service passing in a shipment request and returning a shipment reply
                ProcessShipmentReply reply = service.processShipment(request);
                // check if the call was successful
                if ((reply.HighestSeverity != NotificationSeverityType.ERROR) && (reply.HighestSeverity != NotificationSeverityType.FAILURE))
                {
                    GenerateShipmentLabel(reply, mbi, ref isLabelGenerated);
                }
                else
                {
                    List<Notification> notificaitons = new List<Notification>(reply.Notifications);
                    StringBuilder sbMessages = new StringBuilder(string.Format("Error occurred while generating FedEx label for client {0} and {1} : {2}<br/>", mbi.Client.AppCode, mailPiece.BusinessFriendlyIdName, mailPiece.BusinessFriendlyIdValue));
                    notificaitons.ForEach(n => sbMessages = sbMessages.AppendLine(n.Message));
                    log.Error(sbMessages.ToString());
                    mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                    mailPiece.ErrorCode = OutComeCodes.Invalid_ShippingDetail;

                    // TODO: Check other props of notification. Sometimes rarely in error message FedEx exception also comes. So, we have to replace all new lines in message with space.
                    mailPiece.ErrorDesc = string.Format("{0}|{1}", OutComeCodes.Invalid_ShippingDetail, reply.Notifications[0].Message);
                    isLabelGenerated = false;
                }
            }
            catch (Exception ex)
            {
                var error = "Error occured while generating FedEx label for MailPieceId : {0} And Exception : {1}".Fill(mailPiece.Id, ex);
                log.Error(error);
                mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                mailPiece.ErrorDesc = string.Format("{0}|{1}", OutComeCodes.Unknown_Error, ex.Message);
                SendUnexpectedExceptionEmail(error);
                isLabelGenerated = false;
            }

            return isLabelGenerated;
        }

        private void SendUnexpectedExceptionEmail(string exception)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(Config.Monitor_Exception_Email_To);
                        msg.CC.Add(Config.Monitor_Exception_Email_Cc);
                        msg.Subject = "{0} : {1}".Fill(Config.Common_Environment, Config.Monitor_ExceptionEmail_Subject);
                        msg.Priority = MailPriority.High;
                        msg.Body = "Unexpected exception occured during generating FedEx label :" + Environment.NewLine + exception;
                        smtpClient.Send(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private ProcessShipmentRequest CreateShipmentRequest(MailPieceDTO mailPiece, FedExAccountInfo fedExAccInfo, DocType docType, DateTime fedExShipTimeStamp)
        {
            // Build the ShipmentRequest
            ProcessShipmentRequest shipRequest = new ProcessShipmentRequest();
            SetShipmentDetails(shipRequest, mailPiece, fedExAccInfo, docType, fedExShipTimeStamp);
            SetPackageLineItems(shipRequest);

            if (!string.IsNullOrEmpty(docType.ShippingSpecialService))
            {
                SetSpecialServiceRequestLineItems(shipRequest, docType.ShippingSpecialService, fedExShipTimeStamp);
            }
            return shipRequest;
        }

        private void SetShipmentDetails(ProcessShipmentRequest request, MailPieceDTO mailPiece, FedExAccountInfo fedExAccInfo, DocType docType, DateTime fedExShipTimeStamp)
        {
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = fedExAccInfo.WebAuthenticationKey;
            request.WebAuthenticationDetail.UserCredential.Password = fedExAccInfo.WebAuthenticationPassword; // Replace "XXX" with the Password
            //
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = fedExAccInfo.AccountNumber; // Replace "XXX" with the client's account number
            request.ClientDetail.MeterNumber = fedExAccInfo.MeterNumber; // Replace "XXX" with the client's meter number

            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = mailPiece.Id.ToString(); // The client will get the same value back in the response
            //
            request.Version = new VersionId(); // WSDL version information, value is automatically set from wsdl
            //          
            request.RequestedShipment = new RequestedShipment();
            request.RequestedShipment.ShipTimestamp = fedExShipTimeStamp; // Ship date and time
            request.RequestedShipment.DropoffType = DropoffType.REGULAR_PICKUP;

            request.RequestedShipment.ServiceType = (AM.Core.ShipWebServiceRef.ServiceType)Enum.Parse(typeof(AM.Core.ShipWebServiceRef.ServiceType), docType.ShippingServiceType.ToString());

            // For FedEx GROUND packages the packaging type must be "YOUR_PACKAGING".
            if (docType.ShippingServiceType == ShippingServiceType.FEDEX_GROUND.ToString() || docType.ShippingServiceType == ShippingServiceType.GROUND_HOME_DELIVERY.ToString())
            {
                request.RequestedShipment.PackagingType = PackagingType.YOUR_PACKAGING;
            }
            else
            {
                request.RequestedShipment.PackagingType = PackagingType.FEDEX_ENVELOPE;
            }

            // Packaging type FEDEX_BOK, FEDEX_PAK, FEDEX_TUBE, YOUR_PACKAGING, ...
            request.RequestedShipment.TotalWeight = new Weight(); // Total weight information
            request.RequestedShipment.TotalWeight.Value = 1.0M;
            request.RequestedShipment.TotalWeight.Units = WeightUnits.LB;
            request.RequestedShipment.RateRequestTypes = new RateRequestType[1] { RateRequestType.ACCOUNT }; // Rate types requested LIST, MULTIWEIGHT, ...
            request.RequestedShipment.PackageCount = "1";
            request.RequestedShipment.PackageDetail = RequestedPackageDetailType.INDIVIDUAL_PACKAGES;
            request.RequestedShipment.PackageDetailSpecified = true;

            SetSender(request, mailPiece);
            //
            SetRecipient(request, mailPiece, docType);
            //
            SetPayment(request, fedExAccInfo);
            //
            SetLabelDetails(request);
        }

        private void SetPayment(ProcessShipmentRequest request, FedExAccountInfo fedExAccInfo)
        {
            request.RequestedShipment.ShippingChargesPayment = new Payment(); // Payment information
            request.RequestedShipment.ShippingChargesPayment.PaymentType = PaymentType.SENDER;
            request.RequestedShipment.ShippingChargesPayment.Payor = new Payor();
            request.RequestedShipment.ShippingChargesPayment.Payor.AccountNumber = fedExAccInfo.AccountNumber; // Replace "XXX" with the payor account number
            request.RequestedShipment.ShippingChargesPayment.Payor.CountryCode = Config.Core_CountryCode;
        }

        private void SetLabelDetails(ProcessShipmentRequest request)
        {
            request.RequestedShipment.LabelSpecification = new LabelSpecification(); // Label specification
            request.RequestedShipment.LabelSpecification.ImageType = ShippingDocumentImageType.PNG;
            request.RequestedShipment.LabelSpecification.ImageTypeSpecified = true;
            request.RequestedShipment.LabelSpecification.LabelFormatType = LabelFormatType.COMMON2D;
            request.RequestedShipment.LabelSpecification.LabelStockType = LabelStockType.PAPER_4X9;
            request.RequestedShipment.LabelSpecification.LabelStockTypeSpecified = true;
            request.RequestedShipment.LabelSpecification.LabelPrintingOrientation = LabelPrintingOrientationType.BOTTOM_EDGE_OF_TEXT_FIRST;
            request.RequestedShipment.LabelSpecification.LabelPrintingOrientationSpecified = true;

        }

        private void SetRecipient(ProcessShipmentRequest request, MailPieceDTO mailPiece, DocType docType)
        {
            request.RequestedShipment.Recipient = new Party(); // Recipient information
            request.RequestedShipment.Recipient.Contact = new Contact();
            request.RequestedShipment.Recipient.Contact.PersonName = string.Format("{0} {1}", mailPiece.RcptFirstName, mailPiece.RcptLastName);
            //request.RequestedShipment.Recipient.Contact.CompanyName = loan.RecipientCompanyName;
            request.RequestedShipment.Recipient.Contact.PhoneNumber = string.IsNullOrEmpty(mailPiece.RcptPhone) ? "0000000000" : mailPiece.RcptPhone;
            request.RequestedShipment.Recipient.Address = new ShipWebServiceRef.Address();
            request.RequestedShipment.Recipient.Address.StreetLines = new string[] { mailPiece.RcptAddressStreet, mailPiece.RcptAddressAptOrSuiteOrOther };
            request.RequestedShipment.Recipient.Address.City = mailPiece.RcptAddressCity;
            request.RequestedShipment.Recipient.Address.StateOrProvinceCode = mailPiece.RcptAddressState;// This always has to be a 2 letter code.
            request.RequestedShipment.Recipient.Address.PostalCode = mailPiece.RcptAddressZIP;
            request.RequestedShipment.Recipient.Address.CountryCode = Config.Core_CountryCode;
            request.RequestedShipment.Recipient.Address.Residential = true;

            // For FedEx GROUND_HOME_DELIVERY packages we must specify below flag.
            if (docType.ShippingServiceType == ShippingServiceType.GROUND_HOME_DELIVERY.ToString())
            {
                request.RequestedShipment.Recipient.Address.ResidentialSpecified = true;
            }

        }

        private void SetSender(ProcessShipmentRequest request, MailPieceDTO mailPiece)
        {
            request.RequestedShipment.Shipper = new Party(); // Sender information
            request.RequestedShipment.Shipper.Contact = new Contact();
            request.RequestedShipment.Shipper.Contact.PersonName = string.Format("{0} {1}", mailPiece.RetRcptFirstName, mailPiece.RetRcptLastName);
            request.RequestedShipment.Shipper.Contact.CompanyName = mailPiece.RetRcptOrganization;
            request.RequestedShipment.Shipper.Contact.PhoneNumber = string.IsNullOrEmpty(mailPiece.RcptPhone) ? "0000000000" : mailPiece.RcptPhone;
            request.RequestedShipment.Shipper.Address = new ShipWebServiceRef.Address();
            request.RequestedShipment.Shipper.Address.StreetLines = new string[] { mailPiece.RetRcptAddressStreet, mailPiece.RetRcptAddressAptOrSuiteOrOther };

            request.RequestedShipment.Shipper.Address.City = mailPiece.RetRcptAddressCity;
            request.RequestedShipment.Shipper.Address.StateOrProvinceCode = mailPiece.RetRcptAddressState;
            request.RequestedShipment.Shipper.Address.PostalCode = mailPiece.RetRcptAddressZIP;
            request.RequestedShipment.Shipper.Address.CountryCode = Config.Core_CountryCode;
        }

        private void SetPackageLineItems(ProcessShipmentRequest request)
        {
            request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[1];
            request.RequestedShipment.RequestedPackageLineItems[0] = new RequestedPackageLineItem();
            request.RequestedShipment.RequestedPackageLineItems[0].SequenceNumber = "1";
            request.RequestedShipment.RequestedPackageLineItems[0].Weight = new Weight(); // Package weight information
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.Value = 1.0M;
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.Units = WeightUnits.LB;

            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested = new PackageSpecialServicesRequested();
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.SpecialServiceTypes = new PackageSpecialServiceType[1];
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.SpecialServiceTypes[0] = PackageSpecialServiceType.SIGNATURE_OPTION;
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.SignatureOptionDetail = new SignatureOptionDetail();
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.SignatureOptionDetail.OptionType = SignatureOptionType.INDIRECT;

        }

        private void SetSpecialServiceRequestLineItems(ProcessShipmentRequest request, string shipmentSpecialService, DateTime fedexShipTimeStamp)
        {
            string[] specialServices = shipmentSpecialService.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            request.RequestedShipment.SpecialServicesRequested = new ShipmentSpecialServicesRequested();
            List<ShipmentSpecialServiceType> specialServiceList = new List<ShipmentSpecialServiceType>();
            for (int i = 0; i < specialServices.Length; i++)
            {
                //Here we have to check that If SATURDAY_DELIVERY option is enabled then ShipTimeStamp day should be Friday, otherwise it will considered as an error for that mail piece.
                if (specialServices[i] == ShipmentSpecialServiceType.SATURDAY_DELIVERY.ToString())
                {
                    if (fedexShipTimeStamp.DayOfWeek == DayOfWeek.Friday)
                    {
                        specialServiceList.Add((ShipmentSpecialServiceType)Enum.Parse(typeof(ShipmentSpecialServiceType), specialServices[i]));
                    }
                }
                else if (specialServices[i] == ShipmentSpecialServiceType.SATURDAY_PICKUP.ToString())
                {
                    if (fedexShipTimeStamp.DayOfWeek == DayOfWeek.Saturday)
                    {
                        specialServiceList.Add((ShipmentSpecialServiceType)Enum.Parse(typeof(ShipmentSpecialServiceType), specialServices[i]));
                    }
                }
            }
            request.RequestedShipment.SpecialServicesRequested.SpecialServiceTypes = new ShipmentSpecialServiceType[1];
            request.RequestedShipment.SpecialServicesRequested.SpecialServiceTypes = specialServiceList.ToArray();
        }

        // FedEx shipping service will return the label in landscape orientation and with different margins etc.; so we will create PDF having content in center area from the image returned by FedEx shipping service.
        private void GenerateShipmentLabel(ProcessShipmentReply reply, MailBatchInfo mbi, ref  bool isLabelGenerated)
        {
            string LabelFileName = null;
            FileStream LabelFile = null;

            // Rate details for each package
            foreach (CompletedPackageDetail packageDetail in reply.CompletedShipmentDetail.CompletedPackageDetails)
            {
                LabelFileName = Path.Combine(mbi.PnSRequestPath, string.Format("{0}.pdf", packageDetail.TrackingIds[0].TrackingNumber));
                LabelFile = new FileStream(LabelFileName, FileMode.Create);

                using (Document doc = new Document(PageSize.LETTER))
                {
                    PdfWriter writer1 = PdfWriter.GetInstance(doc, LabelFile);
                    doc.Open();
                    var logo1 = Image.GetInstance(packageDetail.Label.Parts[0].Image);
                    logo1.Alignment = Element.ALIGN_CENTER;
                    logo1.ScaleToFit(430, 600);
                    // logo1.SetDpi(600, 600);
                    logo1.Border = 0;
                    PdfPCell image1Cell = new PdfPCell();
                    image1Cell.AddElement(logo1);
                    image1Cell.Border = PdfPCell.NO_BORDER;
                    image1Cell.PaddingLeft = Utilities.InchesToPoints(-0.5f);
                    image1Cell.PaddingTop = Utilities.InchesToPoints(1.5f);
                    image1Cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    PdfPTable table = new PdfPTable(1);
                    table.AddCell(image1Cell);
                    doc.Add(table);
                    doc.Close();
                }

                GeneratedFedExLabelPath = LabelFileName;
                isLabelGenerated = true;
            }
        }
    }
}
