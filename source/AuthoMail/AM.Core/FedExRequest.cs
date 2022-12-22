using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AM.Core.FedExEnum;

namespace AM.Core
{
    class FedExRequest
    {
        public class Root
        {
            [JsonProperty("mergeLabelDocOption")]
            public MergeLabelDocOption MergeLabelDocOption { get; set; }

            [JsonProperty("requestedShipment")]
            public RequestedShipment RequestedShipment { get; set; }

            [JsonProperty("labelResponseOptions")]
            public LabelResponseOptions LabelResponseOptions { get; set; }

            [JsonProperty("accountNumber")]
            public AccountNumber AccountNumber { get; set; }

            [JsonProperty("shipAction")]
            public ShipAction ShipAction { get; set; }

            [JsonProperty("processingOptionType")]
            public ProcessingOptionType ProcessingOptionType { get; set; }

            [JsonProperty("oneLabelAtATime")]
            public bool OneLabelAtATime { get; set; }
        }

        public class AccountNumber
        {
            [JsonProperty("value")]
            public string Value { get; set; }
        }

        public class AdditionalLabel
        {
            [JsonProperty("type")]
            public AdditionalLabelsType Type { get; set; }

            [JsonProperty("count")]
            public int Count { get; set; }
        }

        public class AdditionalMeasure
        {
            [JsonProperty("quantity")]
            public double Quantity { get; set; }

            [JsonProperty("units")]
            public string Units { get; set; }
        }

        public class Address
        {
            [JsonProperty("streetLines")]
            public List<string> StreetLines { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("stateOrProvinceCode")]
            public string StateOrProvinceCode { get; set; }

            [JsonProperty("postalCode")]
            public string PostalCode { get; set; }

            [JsonProperty("countryCode")]
            public string CountryCode { get; set; }

            [JsonProperty("residential")]
            public bool Residential { get; set; }
        }

        public class AddTransportationChargesDetail
        {
            [JsonProperty("rateType")]
            public RateType RateType { get; set; }

            [JsonProperty("rateLevelType")]
            public RateLevelType RateLevelType { get; set; }

            [JsonProperty("chargeLevelType")]
            public ChargeLevelType ChargeLevelType { get; set; }

            [JsonProperty("chargeType")]
            public ChargeType ChargeType { get; set; }
        }

        public class AlcoholDetail
        {
            [JsonProperty("alcoholRecipientType")]
            public AlcoholRecipientType AlcoholRecipientType { get; set; }

            [JsonProperty("shipperAgreementType")]
            public string ShipperAgreementType { get; set; }
        }

        public class AttachedDocument
        {
            [JsonProperty("documentType")]
            public DocumentType DocumentType { get; set; }

            [JsonProperty("documentReference")]
            public string DocumentReference { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("documentId")]
            public string DocumentId { get; set; }
        }

        public class Barcoded
        {
            [JsonProperty("symbology")]
            public BarcodeSymbologyType Symbology { get; set; }

            [JsonProperty("specification")]
            public Specification Specification { get; set; }
        }

        public class BatteryDetail
        {
            [JsonProperty("batteryPackingType")]
            public BatteryPackingType BatteryPackingType { get; set; }

            [JsonProperty("batteryRegulatoryType")]
            public string BatteryRegulatoryType { get; set; }

            [JsonProperty("batteryMaterialType")]
            public BatteryMaterialType BatteryMaterialType { get; set; }
        }

        public class BillingDetails
        {
            [JsonProperty("billingCode")]
            public string BillingCode { get; set; }

            [JsonProperty("billingType")]
            public string BillingType { get; set; }

            [JsonProperty("aliasId")]
            public string AliasId { get; set; }

            [JsonProperty("accountNickname")]
            public string AccountNickname { get; set; }

            [JsonProperty("accountNumber")]
            public string AccountNumber { get; set; }

            [JsonProperty("accountNumberCountryCode")]
            public string AccountNumberCountryCode { get; set; }
        }

        public class BlanketPeriod
        {
            [JsonProperty("begins")]
            public string Begins { get; set; }

            [JsonProperty("ends")]
            public string Ends { get; set; }
        }

        public class Broker
        {
            [JsonProperty("broker")]
            public Brokers Brokers { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }

        public class Brokers
        {
            [JsonProperty("address")]
            public Address Address { get; set; }

            [JsonProperty("contact")]
            public Contact Contact { get; set; }

            [JsonProperty("accountNumber")]
            public AccountNumber AccountNumber { get; set; }

            [JsonProperty("tins")]
            public List<Tin> Tins { get; set; }

            [JsonProperty("deliveryInstructions")]
            public string DeliveryInstructions { get; set; }
        }

        public class CertificateOfOrigin
        {
            [JsonProperty("customerImageUsages")]
            public List<CustomerImageUsage> CustomerImageUsages { get; set; }

            [JsonProperty("documentFormat")]
            public DocumentFormat DocumentFormat { get; set; }
        }

        public class CodCollectionAmount
        {
            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }
        }

        public class CodRecipient
        {
            [JsonProperty("address")]
            public Address Address { get; set; }

            [JsonProperty("contact")]
            public Contact Contact { get; set; }

            [JsonProperty("accountNumber")]
            public AccountNumber AccountNumber { get; set; }

            [JsonProperty("tins")]
            public List<Tin> Tins { get; set; }
        }

        public class CommercialInvoice
        {
            [JsonProperty("originatorName")]
            public string OriginatorName { get; set; }

            [JsonProperty("comments")]
            public List<string> Comments { get; set; }

            [JsonProperty("customerReferences")]
            public List<CustomerReference> CustomerReferences { get; set; }

            [JsonProperty("taxesOrMiscellaneousCharge")]
            public TaxesOrMiscellaneousCharge TaxesOrMiscellaneousCharge { get; set; }

            [JsonProperty("taxesOrMiscellaneousChargeType")]
            public TaxesOrMiscellaneousChargeType TaxesOrMiscellaneousChargeType { get; set; }

            [JsonProperty("freightCharge")]
            public FreightCharge FreightCharge { get; set; }

            [JsonProperty("packingCosts")]
            public PackingCosts PackingCosts { get; set; }

            [JsonProperty("handlingCosts")]
            public HandlingCosts HandlingCosts { get; set; }

            [JsonProperty("declarationStatement")]
            public string DeclarationStatement { get; set; }

            [JsonProperty("termsOfSale")]
            public string TermsOfSale { get; set; }

            [JsonProperty("specialInstructions")]
            public string SpecialInstructions { get; set; }

            [JsonProperty("shipmentPurpose")]
            public ShipmentPurpose ShipmentPurpose { get; set; }

            [JsonProperty("emailNotificationDetail")]
            public EmailNotificationDetail EmailNotificationDetail { get; set; }
        }

        public class CommercialInvoiceDetail
        {
            [JsonProperty("customerImageUsages")]
            public List<CustomerImageUsage> CustomerImageUsages { get; set; }

            [JsonProperty("documentFormat")]
            public DocumentFormat DocumentFormat { get; set; }
        }

        public class Commodity
        {
            [JsonProperty("unitPrice")]
            public UnitPrice UnitPrice { get; set; }

            [JsonProperty("additionalMeasures")]
            public List<AdditionalMeasure> AdditionalMeasures { get; set; }

            [JsonProperty("numberOfPieces")]
            public int NumberOfPieces { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }

            [JsonProperty("quantityUnits")]
            public string QuantityUnits { get; set; }

            [JsonProperty("customsValue")]
            public CustomsValue CustomsValue { get; set; }

            [JsonProperty("countryOfManufacture")]
            public string CountryOfManufacture { get; set; }

            [JsonProperty("cIMarksAndNumbers")]
            public string CIMarksAndNumbers { get; set; }

            [JsonProperty("harmonizedCode")]
            public string HarmonizedCode { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("weight")]
            public Weight Weight { get; set; }

            [JsonProperty("exportLicenseNumber")]
            public string ExportLicenseNumber { get; set; }

            [JsonProperty("exportLicenseExpirationDate")]
            public DateTime ExportLicenseExpirationDate { get; set; }

            [JsonProperty("partNumber")]
            public string PartNumber { get; set; }

            [JsonProperty("purpose")]
            public ShipmentPurpose Purpose { get; set; }

            [JsonProperty("usmcaDetail")]
            public UsmcaDetail UsmcaDetail { get; set; }
        }

        public class Contact
        {
            [JsonProperty("personName")]
            public string PersonName { get; set; }

            [JsonProperty("emailAddress")]
            public string EmailAddress { get; set; }

            [JsonProperty("phoneExtension")]
            public string PhoneExtension { get; set; }

            [JsonProperty("phoneNumber")]
            public string PhoneNumber { get; set; }

            [JsonProperty("companyName")]
            public string CompanyName { get; set; }

            [JsonProperty("faxNumber")]
            public int FaxNumber { get; set; }
        }

        public class ContentRecord
        {
            [JsonProperty("itemNumber")]
            public string ItemNumber { get; set; }

            [JsonProperty("receivedQuantity")]
            public int ReceivedQuantity { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("partNumber")]
            public string PartNumber { get; set; }
        }

        public class CustomerImageUsage
        {
            [JsonProperty("id")]
            public CustomerImageUsageId Id { get; set; }

            [JsonProperty("type")]
            public CustomerImageUsageType Type { get; set; }

            [JsonProperty("providedImageType")]
            public CustomerImageUsageType ProvidedImageType { get; set; }
        }

        public class CustomerReference
        {
            [JsonProperty("customerReferenceType")]
            public CustomerReferenceType CustomerReferenceType { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }
        }

        public class CustomerSpecifiedDetail
        {
            [JsonProperty("maskedData")]
            public List<CustomerMaskedData> MaskedData { get; set; }

            [JsonProperty("regulatoryLabels")]
            public List<RegulatoryLabel> RegulatoryLabels { get; set; }

            [JsonProperty("additionalLabels")]
            public List<AdditionalLabel> AdditionalLabels { get; set; }

            [JsonProperty("docTabContent")]
            public DocTabContent DocTabContent { get; set; }
        }

        public class CustomsClearanceDetail
        {
            [JsonProperty("regulatoryControls")]
            public RegulatoryControls RegulatoryControls { get; set; }

            [JsonProperty("brokers")]
            public List<Broker> Brokers { get; set; }

            [JsonProperty("commercialInvoice")]
            public CommercialInvoice CommercialInvoice { get; set; }

            [JsonProperty("freightOnValue")]
            public FreightOnValue FreightOnValue { get; set; }

            [JsonProperty("dutiesPayment")]
            public DutiesPayment DutiesPayment { get; set; }

            [JsonProperty("commodities")]
            public List<Commodity> Commodities { get; set; }

            [JsonProperty("isDocumentOnly")]
            public bool IsDocumentOnly { get; set; }

            [JsonProperty("recipientCustomsId")]
            public RecipientCustomsId RecipientCustomsId { get; set; }

            [JsonProperty("customsOption")]
            public CustomsOption CustomsOption { get; set; }

            [JsonProperty("importerOfRecord")]
            public ImporterOfRecord ImporterOfRecord { get; set; }

            [JsonProperty("generatedDocumentLocale")]
            public string GeneratedDocumentLocale { get; set; }

            [JsonProperty("exportDetail")]
            public ExportDetail ExportDetail { get; set; }

            [JsonProperty("totalCustomsValue")]
            public TotalCustomsValue TotalCustomsValue { get; set; }

            [JsonProperty("partiesToTransactionAreRelated")]
            public bool PartiesToTransactionAreRelated { get; set; }

            [JsonProperty("declarationStatementDetail")]
            public DeclarationStatementDetail DeclarationStatementDetail { get; set; }

            [JsonProperty("insuranceCharge")]
            public InsuranceCharge InsuranceCharge { get; set; }
        }

        public class CustomsOption
        {
            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("type")]
            public CustomsOptionType Type { get; set; }
        }

        public class CustomsValue
        {
            [JsonProperty("amount")]
            public string Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }
        }

        public class DangerousGoodsDetail
        {
            [JsonProperty("cargoAircraftOnly")]
            public bool CargoAircraftOnly { get; set; }

            [JsonProperty("accessibility")]
            public AccessibilityType Accessibility { get; set; }

            [JsonProperty("options")]
            public List<DangerousGoodsOptions> Options { get; set; }
        }

        public class DeclarationStatementDetail
        {
            [JsonProperty("usmcaLowValueStatementDetail")]
            public UsmcaLowValueStatementDetail UsmcaLowValueStatementDetail { get; set; }
        }

        public class DeclaredValue
        {
            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }
        }

        public class DeliveryOnInvoiceAcceptanceDetail
        {
            [JsonProperty("recipient")]
            public Recipient Recipient { get; set; }
        }

        public class DestinationControlDetail
        {
            [JsonProperty("endUser")]
            public string EndUser { get; set; }

            [JsonProperty("statementTypes")]
            public DestinationControlStatementType StatementTypes { get; set; }

            [JsonProperty("destinationCountries")]
            public List<string> DestinationCountries { get; set; }
        }

        public class Dimensions
        {
            [JsonProperty("length")]
            public int Length { get; set; }

            [JsonProperty("width")]
            public int Width { get; set; }

            [JsonProperty("height")]
            public int Height { get; set; }

            [JsonProperty("units")]
            public Units Units { get; set; }
        }

        public class Disposition
        {
            [JsonProperty("eMailDetail")]
            public EMailDetail EMailDetail { get; set; }

            [JsonProperty("dispositionType")]
            public ShippingDocumentDispositionType DispositionType { get; set; }
        }

        public class DocTabContent
        {
            [JsonProperty("docTabContentType")]
            public DocTabContentType DocTabContentType { get; set; }

            [JsonProperty("zone001")]
            public Zone001 Zone001 { get; set; }

            [JsonProperty("barcoded")]
            public Barcoded Barcoded { get; set; }
        }

        public class DocTabZoneSpecification
        {
            [JsonProperty("zoneNumber")]
            public int ZoneNumber { get; set; }

            [JsonProperty("header")]
            public string Header { get; set; }

            [JsonProperty("dataField")]
            public string DataField { get; set; }

            [JsonProperty("literalValue")]
            public string LiteralValue { get; set; }

            [JsonProperty("justification")]
            public DocTabZoneJustificationType Justification { get; set; }
        }

        public class DocumentFormat
        {
            [JsonProperty("provideInstructions")]
            public bool ProvideInstructions { get; set; }

            [JsonProperty("optionsRequested")]
            public OptionsRequested OptionsRequested { get; set; }

            [JsonProperty("stockType")]
            public string StockType { get; set; }

            [JsonProperty("dispositions")]
            public List<Disposition> Dispositions { get; set; }

            [JsonProperty("locale")]
            public string Locale { get; set; }

            [JsonProperty("docType")]
            public ImageType DocType { get; set; }
        }

        public class DryIceWeight
        {
            [JsonProperty("units")]
            public Units Units { get; set; }

            [JsonProperty("value")]
            public int Value { get; set; }
        }

        public class DutiesPayment
        {
            [JsonProperty("payor")]
            public Payor Payor { get; set; }

            [JsonProperty("billingDetails")]
            public BillingDetails BillingDetails { get; set; }

            [JsonProperty("paymentType")]
            public PaymentType PaymentType { get; set; }
        }

        public class EMailDetail
        {
            [JsonProperty("eMailRecipients")]
            public List<EMailRecipient> EMailRecipients { get; set; }

            [JsonProperty("locale")]
            public string Locale { get; set; }

            [JsonProperty("grouping")]
            public DocumentGrouping Grouping { get; set; }
        }

        public class EmailLabelDetail
        {
            [JsonProperty("recipients")]
            public List<Recipient> Recipients { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }

        public class EmailNotificationDetail
        {
            [JsonProperty("aggregationType")]
            public AggregationType AggregationType { get; set; }

            [JsonProperty("emailNotificationRecipients")]
            public List<EmailNotificationRecipient> EmailNotificationRecipients { get; set; }

            [JsonProperty("personalMessage")]
            public string PersonalMessage { get; set; }

            [JsonProperty("emailAddress")]
            public string EmailAddress { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("recipientType")]
            public EmailNotificationRecipientType RecipientType { get; set; }
        }

        public class EmailNotificationRecipient
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("emailNotificationRecipientType")]
            public EmailNotificationRecipientType EmailNotificationRecipientType { get; set; }

            [JsonProperty("emailAddress")]
            public string EmailAddress { get; set; }

            [JsonProperty("notificationFormatType")]
            public NotificationFormatType NotificationFormatType { get; set; }

            [JsonProperty("notificationType")]
            public string NotificationType { get; set; }

            [JsonProperty("locale")]
            public string Locale { get; set; }

            [JsonProperty("notificationEventType")]
            public List<NotificationEventType> NotificationEventType { get; set; }
        }

        public class EMailRecipient
        {
            [JsonProperty("emailAddress")]
            public string EmailAddress { get; set; }

            [JsonProperty("recipientType")]
            public EmailNotificationRecipientType RecipientType { get; set; }
        }

        public class EtdDetail
        {
            [JsonProperty("attributes")]
            public List<string> Attributes { get; set; }

            [JsonProperty("attachedDocuments")]
            public List<AttachedDocument> AttachedDocuments { get; set; }

            [JsonProperty("requestedDocumentTypes")]
            public List<RequestedDocumentTypes> RequestedDocumentTypes { get; set; }
        }

        public class ExportDetail
        {
            [JsonProperty("destinationControlDetail")]
            public DestinationControlDetail DestinationControlDetail { get; set; }

            [JsonProperty("b13AFilingOption")]
            public B13AFilingOptionType B13AFilingOption { get; set; }

            [JsonProperty("exportComplianceStatement")]
            public string ExportComplianceStatement { get; set; }

            [JsonProperty("permitNumber")]
            public string PermitNumber { get; set; }
        }

        public class ExpressFreightDetail
        {
            [JsonProperty("bookingConfirmationNumber")]
            public string BookingConfirmationNumber { get; set; }

            [JsonProperty("shippersLoadAndCount")]
            public int ShippersLoadAndCount { get; set; }

            [JsonProperty("packingListEnclosed")]
            public bool PackingListEnclosed { get; set; }
        }

        public class FinancialInstitutionContactAndAddress
        {
            [JsonProperty("address")]
            public Address Address { get; set; }

            [JsonProperty("contact")]
            public Contact Contact { get; set; }
        }

        public class FixedValue
        {
            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }
        }

        public class FreightCharge
        {
            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }
        }

        public class GeneralAgencyAgreementDetail
        {
            [JsonProperty("documentFormat")]
            public DocumentFormat DocumentFormat { get; set; }
        }

        public class HandlingCosts
        {
            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }
        }

        public class HoldAtLocationDetail
        {
            [JsonProperty("locationId")]
            public string LocationId { get; set; }

            [JsonProperty("locationContactAndAddress")]
            public LocationContactAndAddress LocationContactAndAddress { get; set; }

            [JsonProperty("locationType")]
            public LocationType LocationType { get; set; }
        }

        public class HomeDeliveryPremiumDetail
        {
            [JsonProperty("phoneNumber")]
            public PhoneNumber PhoneNumber { get; set; }

            [JsonProperty("deliveryDate")]
            public string DeliveryDate { get; set; }

            [JsonProperty("homedeliveryPremiumType")]
            public HomedeliveryPremiumType HomedeliveryPremiumType { get; set; }
        }

        public class ImporterOfRecord
        {
            [JsonProperty("address")]
            public Address Address { get; set; }

            [JsonProperty("contact")]
            public Contact Contact { get; set; }

            [JsonProperty("accountNumber")]
            public AccountNumber AccountNumber { get; set; }

            [JsonProperty("tins")]
            public List<Tin> Tins { get; set; }
        }

        public class InsuranceCharge
        {
            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }
        }

        public class InternationalControlledExportDetail
        {
            [JsonProperty("licenseOrPermitExpirationDate")]
            public string LicenseOrPermitExpirationDate { get; set; }

            [JsonProperty("licenseOrPermitNumber")]
            public string LicenseOrPermitNumber { get; set; }

            [JsonProperty("entryNumber")]
            public string EntryNumber { get; set; }

            [JsonProperty("foreignTradeZoneCode")]
            public string ForeignTradeZoneCode { get; set; }

            [JsonProperty("type")]
            public InternationalControlledExportType Type { get; set; }
        }

        public class InternationalTrafficInArmsRegulationsDetail
        {
            [JsonProperty("licenseOrExemptionNumber")]
            public string LicenseOrExemptionNumber { get; set; }
        }

        public class LabelSpecification
        {
            [JsonProperty("labelFormatType")]
            public LabelFormatType LabelFormatType { get; set; }

            [JsonProperty("labelOrder")]
            public LabelOrder LabelOrder { get; set; }

            [JsonProperty("customerSpecifiedDetail")]
            public CustomerSpecifiedDetail CustomerSpecifiedDetail { get; set; }

            [JsonProperty("printedLabelOrigin")]
            public PrintedLabelOrigin PrintedLabelOrigin { get; set; }

            [JsonProperty("labelStockType")]
            public LabelStockType LabelStockType { get; set; }

            [JsonProperty("labelRotation")]
            public LabelRotation LabelRotation { get; set; }

            [JsonProperty("imageType")]
            public ImageType ImageType { get; set; }

            [JsonProperty("labelPrintingOrientation")]
            public LabelPrintingOrientation LabelPrintingOrientation { get; set; }

            [JsonProperty("returnedDispositionDetail")]
            public bool ReturnedDispositionDetail { get; set; }
        }

        public class LocationContactAndAddress
        {
            [JsonProperty("address")]
            public Address Address { get; set; }

            [JsonProperty("contact")]
            public Contact Contact { get; set; }
        }

        public class MasterTrackingId
        {
            [JsonProperty("formId")]
            public string FormId { get; set; }

            [JsonProperty("trackingIdType")]
            public string TrackingIdType { get; set; }

            [JsonProperty("uspsApplicationId")]
            public string UspsApplicationId { get; set; }

            [JsonProperty("trackingNumber")]
            public string TrackingNumber { get; set; }
        }

        public class Op900Detail
        {
            [JsonProperty("customerImageUsages")]
            public List<CustomerImageUsage> CustomerImageUsages { get; set; }

            [JsonProperty("signatureName")]
            public string SignatureName { get; set; }

            [JsonProperty("documentFormat")]
            public DocumentFormat DocumentFormat { get; set; }
        }

        public class OptionsRequested
        {
            [JsonProperty("options")]
            public List<Options> Options { get; set; }
        }

        public class Origin
        {
            [JsonProperty("address")]
            public Address Address { get; set; }

            [JsonProperty("contact")]
            public Contact Contact { get; set; }
        }

        public class PackageCODDetail
        {
            [JsonProperty("codCollectionAmount")]
            public CodCollectionAmount CodCollectionAmount { get; set; }
        }

        public class PackageSpecialServices
        {
            [JsonProperty("specialServiceTypes")]
            public List<string> SpecialServiceTypes { get; set; }

            [JsonProperty("signatureOptionType")]
            public SignatureOptionType SignatureOptionType { get; set; }

            [JsonProperty("priorityAlertDetail")]
            public PriorityAlertDetail PriorityAlertDetail { get; set; }

            [JsonProperty("signatureOptionDetail")]
            public SignatureOptionDetail SignatureOptionDetail { get; set; }

            [JsonProperty("alcoholDetail")]
            public AlcoholDetail AlcoholDetail { get; set; }

            [JsonProperty("dangerousGoodsDetail")]
            public DangerousGoodsDetail DangerousGoodsDetail { get; set; }

            [JsonProperty("packageCODDetail")]
            public PackageCODDetail PackageCODDetail { get; set; }

            [JsonProperty("pieceCountVerificationBoxCount")]
            public int PieceCountVerificationBoxCount { get; set; }

            [JsonProperty("batteryDetails")]
            public List<BatteryDetail> BatteryDetails { get; set; }

            [JsonProperty("dryIceWeight")]
            public DryIceWeight DryIceWeight { get; set; }
        }

        public class PackingCosts
        {
            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }
        }

        public class Payor
        {
            [JsonProperty("responsibleParty")]
            public ResponsibleParty ResponsibleParty { get; set; }
        }

        public class PendingShipmentDetail
        {
            [JsonProperty("pendingShipmentType")]
            public PendingShipmentType PendingShipmentType { get; set; }

            [JsonProperty("processingOptions")]
            public ProcessingOptions ProcessingOptions { get; set; }

            [JsonProperty("recommendedDocumentSpecification")]
            public RecommendedDocumentSpecification RecommendedDocumentSpecification { get; set; }

            [JsonProperty("emailLabelDetail")]
            public EmailLabelDetail EmailLabelDetail { get; set; }

            [JsonProperty("attachedDocuments")]
            public List<AttachedDocument> AttachedDocuments { get; set; }

            [JsonProperty("expirationTimeStamp")]
            public string ExpirationTimeStamp { get; set; }
        }

        public class PhoneNumber
        {
            [JsonProperty("areaCode")]
            public string AreaCode { get; set; }

            [JsonProperty("localNumber")]
            public string LocalNumber { get; set; }

            [JsonProperty("extension")]
            public string Extension { get; set; }

            [JsonProperty("personalIdentificationNumber")]
            public string PersonalIdentificationNumber { get; set; }
        }

        public class PrintedLabelOrigin
        {
            [JsonProperty("address")]
            public Address Address { get; set; }

            [JsonProperty("contact")]
            public Contact Contact { get; set; }
        }

        public class PriorityAlertDetail
        {
            [JsonProperty("enhancementTypes")]
            public List<string> EnhancementTypes { get; set; }

            [JsonProperty("content")]
            public List<string> Content { get; set; }
        }

        public class ProcessingOptions
        {
            [JsonProperty("options")]
            public List<string> Options { get; set; }
        }

        public class Producer
        {
            [JsonProperty("address")]
            public Address Address { get; set; }

            [JsonProperty("contact")]
            public Contact Contact { get; set; }

            [JsonProperty("accountNumber")]
            public AccountNumber AccountNumber { get; set; }

            [JsonProperty("tins")]
            public List<Tin> Tins { get; set; }
        }

        public class Recipient
        {
            [JsonProperty("address")]
            public Address Address { get; set; }

            [JsonProperty("contact")]
            public Contact Contact { get; set; }

            [JsonProperty("tins")]
            public List<Tin> Tins { get; set; }

            [JsonProperty("deliveryInstructions")]
            public string DeliveryInstructions { get; set; }

            [JsonProperty("emailAddress")]
            public string EmailAddress { get; set; }

            [JsonProperty("optionsRequested")]
            public OptionsRequested OptionsRequested { get; set; }

            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("locale")]
            public string Locale { get; set; }
        }

        public class Recipient2
        {
            [JsonProperty("address")]
            public Address Address { get; set; }

            [JsonProperty("contact")]
            public Contact Contact { get; set; }

            [JsonProperty("tins")]
            public List<Tin> Tins { get; set; }

            [JsonProperty("deliveryInstructions")]
            public string DeliveryInstructions { get; set; }
        }

        public class RecipientCustomsId
        {
            [JsonProperty("type")]
            public RecipientCustomsIdType Type { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }
        }

        public class RecommendedDocumentSpecification
        {
            [JsonProperty("types")]
            public RecommendedDocumentSpecificationType Types { get; set; }
        }

        public class RegulatoryLabel
        {
            [JsonProperty("generationOptions")]
            public GenerationOptions GenerationOptions { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }

        public class RequestedPackageLineItem
        {
            [JsonProperty("sequenceNumber")]
            public string SequenceNumber { get; set; }

            [JsonProperty("subPackagingType")]
            public string SubPackagingType { get; set; }

            [JsonProperty("customerReferences")]
            public List<CustomerReference> CustomerReferences { get; set; }

            [JsonProperty("declaredValue")]
            public DeclaredValue DeclaredValue { get; set; }

            [JsonProperty("weight")]
            public Weight Weight { get; set; }

            [JsonProperty("dimensions")]
            public Dimensions Dimensions { get; set; }

            [JsonProperty("groupPackageCount")]
            public int GroupPackageCount { get; set; }

            [JsonProperty("itemDescriptionForClearance")]
            public string ItemDescriptionForClearance { get; set; }

            [JsonProperty("contentRecord")]
            public List<ContentRecord> ContentRecord { get; set; }

            [JsonProperty("itemDescription")]
            public string ItemDescription { get; set; }

            [JsonProperty("variableHandlingChargeDetail")]
            public VariableHandlingChargeDetail VariableHandlingChargeDetail { get; set; }

            [JsonProperty("packageSpecialServices")]
            public PackageSpecialServices PackageSpecialServices { get; set; }
        }

        public class RequestedShipment
        {
            [JsonProperty("shipDatestamp")]
            public string ShipDatestamp { get; set; }

            [JsonProperty("totalDeclaredValue")]
            public TotalDeclaredValue TotalDeclaredValue { get; set; }

            [JsonProperty("shipper")]
            public Shipper Shipper { get; set; }

            [JsonProperty("soldTo")]
            public SoldTo SoldTo { get; set; }

            [JsonProperty("recipients")]
            public List<Recipient> Recipients { get; set; }

            [JsonProperty("recipientLocationNumber")]
            public string RecipientLocationNumber { get; set; }

            [JsonProperty("pickupType")]
            public PickupType PickupType { get; set; }

            [JsonProperty("serviceType")]
            public ServiceType ServiceType { get; set; }

            [JsonProperty("packagingType")]
            public PackagingType PackagingType { get; set; }

            [JsonProperty("totalWeight")]
            public double TotalWeight { get; set; }

            [JsonProperty("origin")]
            public Origin Origin { get; set; }

            [JsonProperty("shippingChargesPayment")]
            public ShippingChargesPayment ShippingChargesPayment { get; set; }

            [JsonProperty("shipmentSpecialServices")]
            public ShipmentSpecialServices ShipmentSpecialServices { get; set; }

            [JsonProperty("emailNotificationDetail")]
            public EmailNotificationDetail EmailNotificationDetail { get; set; }

            [JsonProperty("expressFreightDetail")]
            public ExpressFreightDetail ExpressFreightDetail { get; set; }

            [JsonProperty("variableHandlingChargeDetail")]
            public VariableHandlingChargeDetail VariableHandlingChargeDetail { get; set; }

            [JsonProperty("customsClearanceDetail")]
            public CustomsClearanceDetail CustomsClearanceDetail { get; set; }

            [JsonProperty("smartPostInfoDetail")]
            public SmartPostInfoDetail SmartPostInfoDetail { get; set; }

            [JsonProperty("blockInsightVisibility")]
            public bool BlockInsightVisibility { get; set; }

            [JsonProperty("labelSpecification")]
            public LabelSpecification LabelSpecification { get; set; }

            [JsonProperty("shippingDocumentSpecification")]
            public ShippingDocumentSpecification ShippingDocumentSpecification { get; set; }

            [JsonProperty("rateRequestType")]
            public List<RateRequestType> RateRequestType { get; set; }

            [JsonProperty("preferredCurrency")]
            public string PreferredCurrency { get; set; }

            [JsonProperty("totalPackageCount")]
            public int TotalPackageCount { get; set; }

            [JsonProperty("masterTrackingId")]
            public MasterTrackingId MasterTrackingId { get; set; }

            [JsonProperty("requestedPackageLineItems")]
            public List<RequestedPackageLineItem> RequestedPackageLineItems { get; set; }
        }

        public class ResponsibleParty
        {
            [JsonProperty("address")]
            public Address Address { get; set; }

            [JsonProperty("contact")]
            public Contact Contact { get; set; }

            [JsonProperty("accountNumber")]
            public AccountNumber AccountNumber { get; set; }

            [JsonProperty("tins")]
            public List<Tin> Tins { get; set; }
        }

        public class ReturnAssociationDetail
        {
            [JsonProperty("shipDatestamp")]
            public string ShipDatestamp { get; set; }

            [JsonProperty("trackingNumber")]
            public string TrackingNumber { get; set; }
        }

        public class ReturnEmailDetail
        {
            [JsonProperty("merchantPhoneNumber")]
            public string MerchantPhoneNumber { get; set; }

            [JsonProperty("allowedSpecialService")]
            public List<AllowedSpecialService> AllowedSpecialService { get; set; }
        }

        public class ReturnInstructionsDetail
        {
            [JsonProperty("customText")]
            public string CustomText { get; set; }

            [JsonProperty("documentFormat")]
            public DocumentFormat DocumentFormat { get; set; }
        }

        public class ReturnShipmentDetail
        {
            [JsonProperty("returnEmailDetail")]
            public ReturnEmailDetail ReturnEmailDetail { get; set; }

            [JsonProperty("rma")]
            public Rma Rma { get; set; }

            [JsonProperty("returnAssociationDetail")]
            public ReturnAssociationDetail ReturnAssociationDetail { get; set; }

            [JsonProperty("returnType")]
            public ReturnType ReturnType { get; set; }
        }

        public class Rma
        {
            [JsonProperty("reason")]
            public string Reason { get; set; }
        }

        public class ShipmentCodAmount
        {
            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }
        }

        public class ShipmentCODDetail
        {
            [JsonProperty("addTransportationChargesDetail")]
            public AddTransportationChargesDetail AddTransportationChargesDetail { get; set; }

            [JsonProperty("codRecipient")]
            public CodRecipient CodRecipient { get; set; }

            [JsonProperty("remitToName")]
            public string RemitToName { get; set; }

            [JsonProperty("codCollectionType")]
            public CodCollectionType CodCollectionType { get; set; }

            [JsonProperty("financialInstitutionContactAndAddress")]
            public FinancialInstitutionContactAndAddress FinancialInstitutionContactAndAddress { get; set; }

            [JsonProperty("codCollectionAmount")]
            public CodCollectionAmount CodCollectionAmount { get; set; }

            [JsonProperty("returnReferenceIndicatorType")]
            public ReturnReferenceIndicatorType ReturnReferenceIndicatorType { get; set; }

            [JsonProperty("shipmentCodAmount")]
            public ShipmentCodAmount ShipmentCodAmount { get; set; }
        }

        public class ShipmentDryIceDetail
        {
            [JsonProperty("totalWeight")]
            public TotalWeight TotalWeight { get; set; }

            [JsonProperty("packageCount")]
            public int PackageCount { get; set; }
        }

        public class ShipmentSpecialServices
        {
            [JsonProperty("specialServiceTypes")]
            public List<ShipmentSpecialServiceType> SpecialServiceTypes { get; set; }

            [JsonProperty("etdDetail")]
            public EtdDetail EtdDetail { get; set; }

            [JsonProperty("returnShipmentDetail")]
            public ReturnShipmentDetail ReturnShipmentDetail { get; set; }

            [JsonProperty("deliveryOnInvoiceAcceptanceDetail")]
            public DeliveryOnInvoiceAcceptanceDetail DeliveryOnInvoiceAcceptanceDetail { get; set; }

            [JsonProperty("internationalTrafficInArmsRegulationsDetail")]
            public InternationalTrafficInArmsRegulationsDetail InternationalTrafficInArmsRegulationsDetail { get; set; }

            [JsonProperty("pendingShipmentDetail")]
            public PendingShipmentDetail PendingShipmentDetail { get; set; }

            [JsonProperty("holdAtLocationDetail")]
            public HoldAtLocationDetail HoldAtLocationDetail { get; set; }

            [JsonProperty("shipmentCODDetail")]
            public ShipmentCODDetail ShipmentCODDetail { get; set; }

            [JsonProperty("shipmentDryIceDetail")]
            public ShipmentDryIceDetail ShipmentDryIceDetail { get; set; }

            [JsonProperty("internationalControlledExportDetail")]
            public InternationalControlledExportDetail InternationalControlledExportDetail { get; set; }

            [JsonProperty("homeDeliveryPremiumDetail")]
            public HomeDeliveryPremiumDetail HomeDeliveryPremiumDetail { get; set; }
        }

        public class Shipper
        {
            [JsonProperty("address")]
            public Address Address { get; set; }

            [JsonProperty("contact")]
            public Contact Contact { get; set; }

            [JsonProperty("tins")]
            public List<Tin> Tins { get; set; }
        }

        public class ShippingChargesPayment
        {
            [JsonProperty("paymentType")]
            public PaymentType PaymentType { get; set; }

            [JsonProperty("payor")]
            public Payor Payor { get; set; }
        }

        public class ShippingDocumentSpecification
        {
            [JsonProperty("generalAgencyAgreementDetail")]
            public GeneralAgencyAgreementDetail GeneralAgencyAgreementDetail { get; set; }

            [JsonProperty("returnInstructionsDetail")]
            public ReturnInstructionsDetail ReturnInstructionsDetail { get; set; }

            [JsonProperty("op900Detail")]
            public Op900Detail Op900Detail { get; set; }

            [JsonProperty("usmcaCertificationOfOriginDetail")]
            public UsmcaCertificationOfOriginDetail UsmcaCertificationOfOriginDetail { get; set; }

            [JsonProperty("usmcaCommercialInvoiceCertificationOfOriginDetail")]
            public UsmcaCommercialInvoiceCertificationOfOriginDetail UsmcaCommercialInvoiceCertificationOfOriginDetail { get; set; }

            [JsonProperty("shippingDocumentTypes")]
            public List<RequestedDocumentTypes> ShippingDocumentTypes { get; set; }

            [JsonProperty("certificateOfOrigin")]
            public CertificateOfOrigin CertificateOfOrigin { get; set; }

            [JsonProperty("commercialInvoiceDetail")]
            public CommercialInvoiceDetail CommercialInvoiceDetail { get; set; }
        }

        public class SignatureOptionDetail
        {
            [JsonProperty("signatureReleaseNumber")]
            public string SignatureReleaseNumber { get; set; }
        }

        public class SmartPostInfoDetail
        {
            [JsonProperty("ancillaryEndorsement")]
            public SmartPostAncillaryEndorsementType AncillaryEndorsement { get; set; }

            [JsonProperty("hubId")]
            public string HubId { get; set; }

            [JsonProperty("indicia")]
            public SmartPostIndiciaType Indicia { get; set; }

            [JsonProperty("specialServices")]
            public SmartPostShipmentSpecialServiceType SpecialServices { get; set; }
        }

        public class SoldTo
        {
            [JsonProperty("address")]
            public Address Address { get; set; }

            [JsonProperty("contact")]
            public Contact Contact { get; set; }

            [JsonProperty("tins")]
            public List<Tin> Tins { get; set; }

            [JsonProperty("accountNumber")]
            public AccountNumber AccountNumber { get; set; }
        }

        public class Specification
        {
            [JsonProperty("zoneNumber")]
            public int ZoneNumber { get; set; }

            [JsonProperty("header")]
            public string Header { get; set; }

            [JsonProperty("dataField")]
            public string DataField { get; set; }

            [JsonProperty("literalValue")]
            public string LiteralValue { get; set; }

            [JsonProperty("justification")]
            public DocTabZoneJustificationType Justification { get; set; }
        }

        public class TaxesOrMiscellaneousCharge
        {
            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }
        }

        public class Tin
        {
            [JsonProperty("number")]
            public string Number { get; set; }

            [JsonProperty("tinType")]
            public TinType TinType { get; set; }

            [JsonProperty("usage")]
            public string Usage { get; set; }

            [JsonProperty("effectiveDate")]
            public DateTime EffectiveDate { get; set; }

            [JsonProperty("expirationDate")]
            public DateTime ExpirationDate { get; set; }
        }

        public class TotalCustomsValue
        {
            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }
        }

        public class TotalDeclaredValue
        {
            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }
        }

        public class TotalWeight
        {
            [JsonProperty("units")]
            public Units Units { get; set; }

            [JsonProperty("value")]
            public int Value { get; set; }
        }

        public class UnitPrice
        {
            [JsonProperty("amount")]
            public double Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }
        }

        public class UsmcaCertificationOfOriginDetail
        {
            [JsonProperty("customerImageUsages")]
            public List<CustomerImageUsage> CustomerImageUsages { get; set; }

            [JsonProperty("documentFormat")]
            public DocumentFormat DocumentFormat { get; set; }

            [JsonProperty("certifierSpecification")]
            public CertifierSpecification CertifierSpecification { get; set; }

            [JsonProperty("importerSpecification")]
            public ImporterSpecification ImporterSpecification { get; set; }

            [JsonProperty("producerSpecification")]
            public ProducerSpecification ProducerSpecification { get; set; }

            [JsonProperty("producer")]
            public Producer Producer { get; set; }

            [JsonProperty("blanketPeriod")]
            public BlanketPeriod BlanketPeriod { get; set; }

            [JsonProperty("certifierJobTitle")]
            public string CertifierJobTitle { get; set; }
        }

        public class UsmcaCommercialInvoiceCertificationOfOriginDetail
        {
            [JsonProperty("customerImageUsages")]
            public List<CustomerImageUsage> CustomerImageUsages { get; set; }

            [JsonProperty("documentFormat")]
            public DocumentFormat DocumentFormat { get; set; }

            [JsonProperty("certifierSpecification")]
            public CertifierSpecification CertifierSpecification { get; set; }

            [JsonProperty("importerSpecification")]
            public ImporterSpecification ImporterSpecification { get; set; }

            [JsonProperty("producerSpecification")]
            public ProducerSpecification ProducerSpecification { get; set; }

            [JsonProperty("producer")]
            public Producer Producer { get; set; }

            [JsonProperty("certifierJobTitle")]
            public string CertifierJobTitle { get; set; }
        }

        public class UsmcaDetail
        {
            [JsonProperty("originCriterion")]
            public OriginCriterion OriginCriterion { get; set; }
        }

        public class UsmcaLowValueStatementDetail
        {
            [JsonProperty("countryOfOriginLowValueDocumentRequested")]
            public bool CountryOfOriginLowValueDocumentRequested { get; set; }

            [JsonProperty("customsRole")]
            public CustomsRole CustomsRole { get; set; }
        }

        public class VariableHandlingChargeDetail
        {
            [JsonProperty("rateType")]
            public RateType RateType { get; set; }

            [JsonProperty("percentValue")]
            public double PercentValue { get; set; }

            [JsonProperty("rateLevelType")]
            public RateLevelType RateLevelType { get; set; }

            [JsonProperty("fixedValue")]
            public FixedValue FixedValue { get; set; }

            [JsonProperty("rateElementBasis")]
            public RateElementBasis RateElementBasis { get; set; }
        }

        public class Weight
        {
            [JsonProperty("units")]
            public string Units { get; set; }

            [JsonProperty("value")]
            public int Value { get; set; }
        }

        public class Zone001
        {
            [JsonProperty("docTabZoneSpecifications")]
            public List<DocTabZoneSpecification> DocTabZoneSpecifications { get; set; }
        }


    }
}
