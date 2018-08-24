using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.Services
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LabelType
    {
        [EnumMember(Value = "BARCODE ONLY")]
        [XmlEnum(Name = "BARCODE ONLY")]
        Barcodeonly,

        [EnumMember(Value = "CROP")]
        [XmlEnum(Name = "CROP")]
        Crop,

        [EnumMember(Value = "4X6LABEL")]
        [XmlEnum(Name = "4X6LABEL")]
        FourXSixLabel,

        [EnumMember(Value = "4X6LABELL")]
        [XmlEnum(Name = "4X6LABELL")]
        FourXSixLabelLandscape,

        [EnumMember(Value = "6X4LABEL")]
        [XmlEnum(Name = "6X4LABEL")]
        SixXFourLabel,

        [EnumMember(Value = "4X6LABELP")]
        [XmlEnum(Name = "4X6LABELP")]
        FourXSixLabelPortrait,

        [EnumMember(Value = "4X6LABELP PAGE")]
        [XmlEnum(Name = "4X6LABELP PAGE")]
        FourXSixLabelPortraitPPAGE,

        [EnumMember(Value = "SEPARATECONTINUEPAGE")]
        [XmlEnum(Name = "SEPARATECONTINUEPAGE")]
        Separatecontinuepage,

        [EnumMember(Value = "4X6LABELZPL")]
        [XmlEnum(Name = "4X6LABELZPL")]
        FourXSixLabelZPL
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ReceiptOption
    {
        [EnumMember(Value = "NONE")]
        [XmlEnum(Name = "NONE")]
        None,

        [EnumMember(Value = "SAME PAGE")]
        [XmlEnum(Name = "SAME PAGE")]
        Samepage,

        [EnumMember(Value = "SEPARATE PAGE")]
        [XmlEnum(Name = "SEPARATE PAGE")]
        Separatepage
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServiceType
    {
        [EnumMember(Value = "PRIORITY EXPRESS")]
        [XmlEnum(Name = "PRIORITY EXPRESS")]
        Priorityexpress,

        [EnumMember(Value = "PRIORITY")]
        [XmlEnum(Name = "PRIORITY")]
        Priority,

        [EnumMember(Value = "FIRST CLASS")]
        [XmlEnum(Name = "FIRST CLASS")]
        Firstclass,

        [EnumMember(Value = "PARCEL SELECT GROUND")]
        [XmlEnum(Name = "PARCEL SELECT GROUND")]
        Parcelselectground,

        [EnumMember(Value = "LIBRARY")]
        [XmlEnum(Name = "LIBRARY")]
        Library,

        [EnumMember(Value = "MEDIA")]
        [XmlEnum(Name = "MEDIA")]
        Media,

        [EnumMember(Value = "BPM")]
        [XmlEnum(Name = "BPM")]
        Bpm,

        [EnumMember(Value = "PRIORITY MAIL CUBIC")]
        [XmlEnum(Name = "PRIORITY MAIL CUBIC")]
        Prioritymailcubic
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
    public enum ExtraService
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
        FLATRATEENVELOPE,

        [EnumMember(Value = "LEGAL FLAT RATE ENVELOPE")]
        [XmlEnum(Name = "LEGAL FLAT RATE ENVELOPE")]
        LEGALFLATRATEENVELOPE,

        [EnumMember(Value = "PADDED FLAT RATE ENVELOPE")]
        [XmlEnum(Name = "PADDED FLAT RATE ENVELOPE")]
        PADDEDFLATRATEENVELOPE,

        [EnumMember(Value = "GIFT CARD FLAT RATE ENVELOPE")]
        [XmlEnum(Name = "GIFT CARD FLAT RATE ENVELOPE")]
        GIFTCARDFLATRATEENVELOPE,

        [EnumMember(Value = "SM FLAT RATE ENVELOPE")]
        [XmlEnum(Name = "SM FLAT RATE ENVELOPE")]
        SMFLATRATEENVELOPE,

        [EnumMember(Value = "WINDOW FLAT RATE ENVELOPE")]
        [XmlEnum(Name = "WINDOW FLAT RATE ENVELOPE")]
        WINDOWFLATRATEENVELOPE,

        [EnumMember(Value = "SM FLAT RATE BOX")]
        [XmlEnum(Name = "SM FLAT RATE BOX")]
        SMFLATRATEBOX,

        [EnumMember(Value = "MD FLAT RATE BOX")]
        [XmlEnum(Name = "MD FLAT RATE BOX")]
        MDFLATRATEBOX,

        [EnumMember(Value = "LG FLAT RATE BOX")]
        [XmlEnum(Name = "LG FLAT RATE BOX")]
        LGFLATRATEBOX,

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
        PACKAGESERVICE,

        [EnumMember(Value = "CUBIC PARCELS")]
        [XmlEnum(Name = "CUBIC PARCELS")]
        CUBICPARCELS,

        [EnumMember(Value = "CUBIC SOFT PACK")]
        [XmlEnum(Name = "CUBIC SOFT PACK")]
        CUBICSOFTPACK
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
}