using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.USPSLib
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LabelSize
    {
        [EnumMember(Value = "BARCODE ONLY")]
        [XmlEnum(Name = "BARCODE ONLY")]
        BARCODE_ONLY,

        [EnumMember(Value = "CROP")]
        [XmlEnum(Name = "CROP")]
        CROP,

        [EnumMember(Value = "4X6LABEL")]
        [XmlEnum(Name = "4X6LABEL")]
        LABEL4X6,

        [EnumMember(Value = "4X6LABELL")]
        [XmlEnum(Name = "4X6LABELL")]
        LABEL4X6L,

        [EnumMember(Value = "6X4LABEL")]
        [XmlEnum(Name = "6X4LABEL")]
        LABEL6X4,

        [EnumMember(Value = "4X6LABELP")]
        [XmlEnum(Name = "4X6LABELP")]
        LABEL4X6P,

        [EnumMember(Value = "4X6LABELP PAGE")]
        [XmlEnum(Name = "4X6LABELP PAGE")]
        LABEL4X6P_PAGE,

        [EnumMember(Value = "SEPARATECONTINUEPAGE")]
        [XmlEnum(Name = "SEPARATECONTINUEPAGE")]
        SEPARATECONTINUEPAGE,

        [EnumMember(Value = "4X6LABELZPL")]
        [XmlEnum(Name = "4X6LABELZPL")]
        LABEL4X6ZPL
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ReceiptOption
    {
        [EnumMember(Value = "NONE")]
        [XmlEnum(Name = "NONE")]
        None,

        [EnumMember(Value = "SAME PAGE")]
        [XmlEnum(Name = "SAME PAGE")]
        SAME_PAGE,

        [EnumMember(Value = "SEPARATE PAGE")]
        [XmlEnum(Name = "SEPARATE PAGE")]
        SEPARATE_PAGE
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServiceType
    {
        [EnumMember(Value = "PRIORITY EXPRESS")]
        [XmlEnum(Name = "PRIORITY EXPRESS")]
        PRIORITY_EXPRESS,

        [EnumMember(Value = "PRIORITY")]
        [XmlEnum(Name = "PRIORITY")]
        PRIORITY,

        [EnumMember(Value = "FIRST CLASS")]
        [XmlEnum(Name = "FIRST CLASS")]
        FIRST_CLASS,

        [EnumMember(Value = "PARCEL SELECT GROUND")]
        [XmlEnum(Name = "PARCEL SELECT GROUND")]
        PARCEL_SELECT_GROUND,

        [EnumMember(Value = "LIBRARY")]
        [XmlEnum(Name = "LIBRARY")]
        LIBRARY,

        [EnumMember(Value = "MEDIA")]
        [XmlEnum(Name = "MEDIA")]
        MEDIA,

        [EnumMember(Value = "BPM")]
        [XmlEnum(Name = "BPM")]
        BPM,

        [EnumMember(Value = "PRIORITY MAIL CUBIC")]
        [XmlEnum(Name = "PRIORITY MAIL CUBIC")]
        PRIORITY_MAIL_CUBIC
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeliveryOption
    {
        [EnumMember(Value = "1")]
        [XmlEnum(Name = "1")]
        NormalDelivery = 1,

        [EnumMember(Value = "2")]
        [XmlEnum(Name = "2")]
        NoWeekendDelivery = 2,

        [EnumMember(Value = "3")]
        [XmlEnum(Name = "3")]
        NoHolidayDelivery = 3,

        [EnumMember(Value = "4")]
        [XmlEnum(Name = "4")]
        NoWeekendorHolidayDelivery = 4,

        [EnumMember(Value = "5")]
        [XmlEnum(Name = "5")]
        NoSaturdayDelivery = 5,

        [EnumMember(Value = "6")]
        [XmlEnum(Name = "6")]
        NoSundayDelivery = 6,

        [EnumMember(Value = "7")]
        [XmlEnum(Name = "7")]
        SundayDelivery = 7,

        [EnumMember(Value = "8")]
        [XmlEnum(Name = "8")]
        HolidayDelivery = 8,

        [EnumMember(Value = "9")]
        [XmlEnum(Name = "9")]
        NoSundayorHolidayDelivery = 9,

        [EnumMember(Value = "E")]
        [XmlEnum(Name = "E")]
        ScheduledEndofDay,

        [EnumMember(Value = "F")]
        [XmlEnum(Name = "F")]
        ScheduledEndofDayNoSun_HolDelivery,

        [EnumMember(Value = "G")]
        [XmlEnum(Name = "G")]
        ScheduledEndofDayNoWeekend_HolDelivery
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExtraServiceType
    {
        [EnumMember(Value = "100")]
        [XmlEnum(Name = "100")]
        Insurance = 100,

        [EnumMember(Value = "101")]
        [XmlEnum(Name = "101")]
        InsurancePME = 101,

        [EnumMember(Value = "102")]
        [XmlEnum(Name = "102")]
        ReturnReceipt = 102,

        [EnumMember(Value = "105")]
        [XmlEnum(Name = "105")]
        CertifiedMail = 105,

        [EnumMember(Value = "110")]
        [XmlEnum(Name = "110")]
        ReturnReceiptElectronic = 110,

        [EnumMember(Value = "118")]
        [XmlEnum(Name = "118")]
        ReturnReceiptExpressMail = 118,

        [EnumMember(Value = "119")]
        [XmlEnum(Name = "119")]
        AdultSignature = 119,

        [EnumMember(Value = "120")]
        [XmlEnum(Name = "120")]
        AdultSignatureRestrictedDelivery = 120,

        [EnumMember(Value = "125")]
        [XmlEnum(Name = "125")]
        InsurancePM = 125,

        [EnumMember(Value = "155")]
        [XmlEnum(Name = "155")]
        USPSTrackingElectronic = 155,

        [EnumMember(Value = "156")]
        [XmlEnum(Name = "156")]
        SignatureConfirmationElectronic = 156,

        [EnumMember(Value = "161")]
        [XmlEnum(Name = "161")]
        DeliveryPME = 161,

        [EnumMember(Value = "170")]
        [XmlEnum(Name = "170")]
        CertifiedMailRestrictedDelivery = 170,

        [EnumMember(Value = "171")]
        [XmlEnum(Name = "171")]
        CertifiedMailAdultSignatureRequired = 171,

        [EnumMember(Value = "172")]
        [XmlEnum(Name = "172")]
        CertifiedMailAdultSignatureRestrictedDelivery = 172,

        [EnumMember(Value = "174")]
        [XmlEnum(Name = "174")]
        SignatureConfirmationElectronicRestrictedDelivery = 174,

        [EnumMember(Value = "177")]
        [XmlEnum(Name = "177")]
        InsuranceRestrictedDelivery = 177,

        [EnumMember(Value = "178")]
        [XmlEnum(Name = "178")]
        InsuranceRestrictedDeliveryPME = 178,

        [EnumMember(Value = "179")]
        [XmlEnum(Name = "179")]
        InsuranceRestrictedDeliveryPM = 179,

        [EnumMember(Value = "190")]
        [XmlEnum(Name = "190")]
        SpecialHandling_Fragile = 190
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ImageType
    {
        [EnumMember(Value = "PDF")]
        [XmlEnum(Name = "PDF")]
        PDF,

        [EnumMember(Value = "TIF")]
        [XmlEnum(Name = "TIF")]
        TIF,

        [EnumMember(Value = "NONE")]
        [XmlEnum(Name = "NONE")]
        None
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Machinable
    {
        [EnumMember(Value = "True")]
        [XmlEnum(Name = "True")]
        True,

        [EnumMember(Value = "False")]
        [XmlEnum(Name = "False")]
        False,

        [EnumMember(Value = "Irregular")]
        [XmlEnum(Name = "Irregular")]
        Irregular
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum HoldForManifest
    {
        [EnumMember(Value = "Y")]
        [XmlEnum(Name = "Y")]
        Y,

        [EnumMember(Value = "N")]
        [XmlEnum(Name = "N")]
        N
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Container
    {
        [EnumMember(Value = "VARIABLE")]
        [XmlEnum(Name = "VARIABLE")]
        VARIABLE,

        [EnumMember(Value = "FLAT RATE ENVELOPE")]
        [XmlEnum(Name = "FLAT RATE ENVELOPE")]
        FLAT_RATE_ENVELOPE,

        [EnumMember(Value = "LEGAL FLAT RATE ENVELOPE")]
        [XmlEnum(Name = "LEGAL FLAT RATE ENVELOPE")]
        LEGAL_FLAT_RATE_ENVELOPE,

        [EnumMember(Value = "PADDED FLAT RATE ENVELOPE")]
        [XmlEnum(Name = "PADDED FLAT RATE ENVELOPE")]
        PADDED_FLAT_RATE_ENVELOPE,

        [EnumMember(Value = "GIFT CARD FLAT RATE ENVELOPE")]
        [XmlEnum(Name = "GIFT CARD FLAT RATE ENVELOPE")]
        GIFT_CARD_FLAT_RATE_ENVELOPE,

        [EnumMember(Value = "SM FLAT RATE ENVELOPE")]
        [XmlEnum(Name = "SM FLAT RATE ENVELOPE")]
        SM_FLAT_RATE_ENVELOPE,

        [EnumMember(Value = "WINDOW FLAT RATE ENVELOPE")]
        [XmlEnum(Name = "WINDOW FLAT RATE ENVELOPE")]
        WINDOW_FLAT_RATE_ENVELOPE,

        [EnumMember(Value = "SM FLAT RATE BOX")]
        [XmlEnum(Name = "SM FLAT RATE BOX")]
        SM_FLAT_RATE_BOX,

        [EnumMember(Value = "MD FLAT RATE BOX")]
        [XmlEnum(Name = "MD FLAT RATE BOX")]
        MD_FLAT_RATE_BOX,

        [EnumMember(Value = "LG FLAT RATE BOX")]
        [XmlEnum(Name = "LG FLAT RATE BOX")]
        LG_FLAT_RATE_BOX,

        [EnumMember(Value = "REGIONALRATEBOXA")]
        [XmlEnum(Name = "REGIONALRATEBOXA")]
        REGIONALRATEBOXA,

        [EnumMember(Value = "REGIONALRATEBOXB")]
        [XmlEnum(Name = "REGIONALRATEBOXB")]
        REGIONALRATEBOXB,

        [EnumMember(Value = "RECTANGULAR")]
        [XmlEnum(Name = "RECTANGULAR")]
        RECTANGULAR,

        [EnumMember(Value = "NONRECTANGULAR")]
        [XmlEnum(Name = "NONRECTANGULAR")]
        NONRECTANGULAR,

        [EnumMember(Value = "PACKAGE SERVICE")]
        [XmlEnum(Name = "PACKAGE SERVICE")]
        PACKAGE_SERVICE,

        [EnumMember(Value = "CUBIC PARCELS")]
        [XmlEnum(Name = "CUBIC PARCELS")]
        CUBIC_PARCELS,

        [EnumMember(Value = "CUBIC SOFT PACK")]
        [XmlEnum(Name = "CUBIC SOFT PACK")]
        CUBIC_SOFT_PACK
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PriceOption
    {
        [EnumMember(Value = "Retail")]
        [XmlEnum(Name = "Retail")]
        Retail,

        [EnumMember(Value = "Commercial Plus")]
        [XmlEnum(Name = "Commercial Plus")]
        CommercialPlus,

        [EnumMember(Value = "Commercial Base")]
        [XmlEnum(Name = "Commercial Base")]
        CommercialBase
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ToContactPreference
    {
        [EnumMember(Value = "EMAIL")]
        [XmlEnum(Name = "EMAIL")]
        EMAIL,

        [EnumMember(Value = "EMAILSMS")]
        [XmlEnum(Name = "EMAILSMS")]
        EMAILSMS,

        [EnumMember(Value = "SMS")]
        [XmlEnum(Name = "SMS")]
        SMS,

        [EnumMember(Value = "WAIVED")]
        [XmlEnum(Name = "WAIVED")]
        WAIVED
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContentType
    {
        [EnumMember(Value = "HAZMAT")]
        [XmlEnum(Name = "HAZMAT")]
        HAZMAT,

        [EnumMember(Value = "CrematedRemains")]
        [XmlEnum(Name = "CrematedRemains")]
        CrematedRemains,

        [EnumMember(Value = "Lives")]
        [XmlEnum(Name = "Lives")]
        Lives,

        [EnumMember(Value = "Perishable")]
        [XmlEnum(Name = "Perishable")]
        Perishable,

        [EnumMember(Value = "Pharmaceuticals")]
        [XmlEnum(Name = "Pharmaceuticals")]
        Pharmaceuticals,

        [EnumMember(Value = "MedicalSupplies")]
        [XmlEnum(Name = "MedicalSupplies")]
        MedicalSupplies
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DestinationEntryFacilityType
    {
        [EnumMember(Value = "None (default)")]
        [XmlEnum(Name = "None (default)")]
        None,

        [EnumMember(Value = "DNDC")]
        [XmlEnum(Name = "DNDC")]
        DNDC,

        [EnumMember(Value = "DSCF")]
        [XmlEnum(Name = "DSCF")]
        DSCF,

        [EnumMember(Value = "DDU")]
        [XmlEnum(Name = "DDU")]
        DDU
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActionCode
    {
        [EnumMember(Value = "M0")]
        [XmlEnum(Name = "M0")]
        MailerOwner,

        [EnumMember(Value = "S0")]
        [XmlEnum(Name = "S0")]
        ServiceProvider
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContentDescription
    {
        [EnumMember(Value = "Bees")]
        [XmlEnum(Name = "Bees")]
        Bees,

        [EnumMember(Value = "DayOldPoultry")]
        [XmlEnum(Name = "DayOldPoultry")]
        DayOldPoultry,

        [EnumMember(Value = "AdultBirds")]
        [XmlEnum(Name = "AdultBirds")]
        AdultBirds,

        [EnumMember(Value = "Other")]
        [XmlEnum(Name = "Other")]
        Other
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RestrictionType
    {
        [EnumMember(Value = "Quarantine")]
        [XmlEnum(Name = "Quarantine")]
        Quarantine,

        [EnumMember(Value = "Sanitary Inspection")]
        [XmlEnum(Name = "Sanitary Inspection")]
        Sanitary_Inspection,

        [EnumMember(Value = "Phytosanitary Inspection")]
        [XmlEnum(Name = "Phytosanitary Inspection")]
        Phytosanitary_Inspection,

        [EnumMember(Value = "Other")]
        [XmlEnum(Name = "Other")]
        Other
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CustomsContentType
    {
        [EnumMember(Value = "MERCHANDISE")]
        [XmlEnum(Name = "MERCHANDISE")]
        MERCHANDISE,

        [EnumMember(Value = "GIFT")]
        [XmlEnum(Name = "GIFT")]
        GIFT,

        [EnumMember(Value = "DOCUMENTS")]
        [XmlEnum(Name = "DOCUMENTS")]
        DOCUMENTS,

        [EnumMember(Value = "SAMPLE")]
        [XmlEnum(Name = "SAMPLE")]
        SAMPLE,

        [EnumMember(Value = "RETURN")]
        [XmlEnum(Name = "RETURN")]
        RETURN,

        [EnumMember(Value = "OTHER")]
        [XmlEnum(Name = "OTHER")]
        OTHER,

        [EnumMember(Value = "HUMANITARIAN")]
        [XmlEnum(Name = "HUMANITARIAN")]
        HUMANITARIAN,

        [EnumMember(Value = "DANGEROUSGOODS")]
        [XmlEnum(Name = "DANGEROUSGOODS")]
        DANGEROUSGOODS
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum NonDeliveryOption
    {
        [EnumMember(Value = "RETURN")]
        [XmlEnum(Name = "RETURN")]
        RETURN,

        [EnumMember(Value = "REDIRECT")]
        [XmlEnum(Name = "REDIRECT")]
        REDIRECT,

        [EnumMember(Value = "ABANDON")]
        [XmlEnum(Name = "ABANDON")]
        ABANDON
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ePostageMailerReporting
    {
        [EnumMember(Value = "1")]
        [XmlEnum(Name = "1")]
        ePostage_sender_information_in_SSF = 1,

        [EnumMember(Value = "2")]
        [XmlEnum(Name = "2")]
        ePostage_sender_information_through_DES = 2,


        [EnumMember(Value = "3")]
        [XmlEnum(Name = "3")]
        ePostage_sender_information_using_child_MID = 3
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum eVSCancelStatus
    {
        [EnumMember(Value = "Cancelled")]
        [XmlEnum(Name = "Cancelled")]
        Cancelled,

        [EnumMember(Value = "Not Cancelled")]
        [XmlEnum(Name = "Not Cancelled")]
        Not_Cancelled
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum eVSCancelReason
    {
        [EnumMember(Value = "Order Cancelled Successfully")]
        [XmlEnum(Name = "Order Cancelled Successfully")]
        Order_Cancelled_Successfully,

        [EnumMember(Value = "Order Already Cancelled")]
        [XmlEnum(Name = "Order Already Cancelled")]
        Order_Already_Cancelled,

        [EnumMember(Value = "Order Not Found")]
        [XmlEnum(Name = "Order Not Found")]
        Order_Not_Found
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum AddressType
    {
        [EnumMember(Value = "INVALID")]
        [XmlEnum(Name = "INVALID")]
        INVALID,

        [EnumMember(Value = "POBOX")]
        [XmlEnum(Name = "POBOX")]
        POBOX,

        [EnumMember(Value = "Military")]
        [XmlEnum(Name = "Military")]
        Military,

        [EnumMember(Value = "Territory")]
        [XmlEnum(Name = "Territory")]
        Territory,

        //DC,Hawaii,Alaska and All Mainlands
        [EnumMember(Value = "DHAM")]
        [XmlEnum(Name = "DHAM")]
        DHAM
    }
}