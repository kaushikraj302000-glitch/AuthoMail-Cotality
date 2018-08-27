using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AM.USPSLib;

namespace AM.Services
{
    public class eVSRequest : IeVSRequest
    {
        public string USERID { get; set; }

        public string PASSWORD { get; set; }

        [XmlElement(ElementName = "ImageParameters")]
        public LabelParameters LabelParameters { get; set; }

        public string FromName { get; set; }

        public string FromFirm { get; set; }

        public string FromAddress1 { get; set; }

        public string FromAddress2 { get; set; }

        public string FromCity { get; set; }

        public string FromState { get; set; }

        public string FromZip5 { get; set; }

        public string FromZip4 { get; set; }

        public string FromPhone { get; set; }

        public string POZipCode { get; set; }

        public bool AllowNonCleansedOriginAddr { get; set; }

        public string ToName { get; set; }

        public string ToFirm { get; set; }

        public string ToAddress1 { get; set; }

        public string ToAddress2 { get; set; }

        public string ToCity { get; set; }

        public string ToState { get; set; }

        public string ToZip5 { get; set; }

        public string ToZip4 { get; set; }

        public string ToPhone { get; set; }

        public bool POBox { get; set; }

        public ToContactPreference ToContactPreference { get; set; }

        public string ToContactMessaging { get; set; }

        public string ToContactEmail { get; set; }

        public bool AllowNonCleansedDestAddr { get; set; }

        public string WeightInOunces { get; set; }

        public ServiceType ServiceType { get; set; }

        public Container Container { get; set; }

        public decimal Width { get; set; }

        public decimal Length { get; set; }

        public decimal Height { get; set; }

        public Machinable Machinable { get; set; }

        public PriceOption PriceOptions { get; set; }

        public decimal InsuredAmount { get; set; }

        public bool AddressServiceRequested { get; set; }

        public ExpressMailOptions ExpressMailOptions { get; set; }

        public string ShipDate { get; set; }

        public string CustomerRefNo { get; set; }

        public string CustomerRefNo2 { get; set; }

        public List<ExtraService> ExtraServices { get; set; }

        public string CRID { get; set; }

        public string MID { get; set; }

        public string LogisticsManagerMID { get; set; }

        public string VendorCode { get; set; }

        public string VendorProductVersionNumber { get; set; }

        public string SenderName { get; set; }

        public string SenderEMail { get; set; }

        public string RecipientName { get; set; }

        public string RecipientEMail { get; set; }

        public ReceiptOption ReceiptOption { get; set; }

        public ImageType ImageType { get; set; }

        public HoldForManifest HoldForManifest { get; set; }

        public bool NineDigitRoutingZip { get; set; }

        public bool ShipInfo { get; set; }

        public bool CarrierRelease { get; set; }

        public bool DropOffTime { get; set; }

        public bool ReturnCommitments { get; set; }

        public bool PrintCustomerRefNo { get; set; }

        public Content Content { get; set; }

        public ActionCode ActionCode { get; set; }

        public bool OptOutOfSPE { get; set; }

        public string SortationLevel { get; set; }

        public DestinationEntryFacilityType DestinationEntryFacilityType { get; set; }

        public eVSCustomsFormsData eVSCustomsFormsData { get; set; }

        public eVSRequest(eVSLabelRequest request)
        {
            //set everything
            this.LabelParameters = request.LabelParameters;
            this.FromName = request.FromName;
            this.FromFirm = request.FromFirm;
            this.FromAddress1 = request.FromAddress1;
            this.FromAddress2 = request.FromAddress2;
            this.FromCity = request.FromCity;
            this.FromState = request.FromState;
            this.FromZip5 = request.FromZip5;
            this.FromZip4 = request.FromZip4;
            this.FromPhone = request.FromPhone;
            this.POZipCode = request.POZipCode;
            this.AllowNonCleansedOriginAddr = request.AllowNonCleansedOriginAddr;
            this.ToName = request.ToName;
            this.ToFirm = request.ToFirm;
            this.ToAddress1 = request.ToAddress1;
            this.ToAddress2 = request.ToAddress2;
            this.ToCity = request.ToCity;
            this.ToState = request.ToState;
            this.ToZip5 = request.ToZip5;
            this.ToZip4 = request.ToZip4;
            this.ToPhone = request.ToPhone;
            this.POBox = request.POBox;
            this.ToContactPreference = request.ToContactPreference;
            this.ToContactMessaging = request.ToContactMessaging;
            this.ToContactEmail = request.ToContactEmail;
            this.AllowNonCleansedDestAddr = request.AllowNonCleansedDestAddr;
            this.WeightInOunces = request.WeightInOunces;
            this.ServiceType = request.ServiceType;
            this.Container = request.Container;
            this.Width = request.Width;
            this.Length = request.Length;
            this.Height = request.Height;
            this.Machinable = request.Machinable;
            this.PriceOptions = request.PriceOptions;
            this.InsuredAmount = request.InsuredAmount;
            this.AddressServiceRequested = request.AddressServiceRequested;
            this.ExpressMailOptions = request.ExpressMailOptions;
            this.ShipDate = request.ShipDate.ToString("dd-MMM-yyyy");
            this.CustomerRefNo = request.CustomerRefNo;
            this.CustomerRefNo2 = request.CustomerRefNo2;
            this.ExtraServices = request.ExtraServices;
            this.CRID = request.CRID;
            this.MID = request.MID;
            this.LogisticsManagerMID = request.LogisticsManagerMID;
            this.VendorCode = request.VendorCode;
            this.VendorProductVersionNumber = request.VendorProductVersionNumber;
            this.SenderName = request.SenderName;
            this.SenderEMail = request.SenderEMail;
            this.RecipientName = request.RecipientName;
            this.RecipientEMail = request.RecipientEMail;
            this.ReceiptOption = request.ReceiptOption;
            this.ImageType = request.ImageType;
            this.HoldForManifest = request.HoldForManifest;
            this.NineDigitRoutingZip = request.NineDigitRoutingZip;
            this.ShipInfo = request.ShipInfo;
            this.CarrierRelease = request.CarrierRelease;
            this.DropOffTime = request.DropOffTime;
            this.ReturnCommitments = request.ReturnCommitments;
            this.PrintCustomerRefNo = request.PrintCustomerRefNo;
            this.Content = request.Content;
            this.ActionCode = request.ActionCode;
            this.OptOutOfSPE = request.OptOutOfSPE;
            this.SortationLevel = request.SortationLevel;
            this.DestinationEntryFacilityType = request.DestinationEntryFacilityType;
        }

        public eVSRequest()
        {

        }
    }
}
