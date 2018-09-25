using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCI.Lib;
using System.IO;

namespace PCI.Service
{
    public class BatchSummary
    {
        public int PrintBatch { get; set; }
        public int ClientId { get; set; }
        public string JobName { get; set; }
        public string PipelineStatus { get; set; }
        public string ValidationStatus { get; set; }
        public string OnValidationErrors { get; set; }
        public string ValidationErrors { get; set; }
        public bool IsReceived { get; set; }
        public DateTime ReceivedOn { get; set; }
        public bool IsDownloaded { get; set; }
        public DateTime DownloadedOn { get; set; }
        public bool IsPrepped { get; set; }
        public DateTime PreppedOn { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime SubmittedOn { get; set; }
        public bool IsMRDFReceived { get; set; }
        public DateTime MRDFReceivedOn { get; set; }
        public bool IsReportPrepped { get; set; }
        public DateTime ReportPreppedOn { get; set; }
        public bool IsReportedBack { get; set; }
        public DateTime ReportedBackOn { get; set; }
        public int JulianDate { get; set; }
        public int Images { get; set; }
        public DateTime DueDate { get; set; }
        public int SLADays { get; set; }
        public string BatchStatus { get; set; }
        public bool IsSuspended { get; set; }
        public DateTime SuspendedOn { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime CancelledOn { get; set; }
        public Guid BatchIdentifier { get; set; }
        public int? DocTypeId { get; set; }
        public string PrintFileName { get; set; }
        public string PrintLocation { get; set; }
    }

    public class BatchInfo
    {
        public string BatchRepo { get; private set; }
        public string BatchZipFilePath { get; private set; }
        public string PnSRequestPath { get; private set; }
        public string PCIRequestPath { get; private set; }
        public string PCIResponsePath { get; private set; }
        public string PCIResponseReturnPDFsPath { get; private set; }
        public string PnSResponsePath { get; private set; }
        public string PnSRequestPDFsPath { get; private set; }
        public string BatchName { get; private set; }
        public string RequestXmlPath { get; private set; }

        public string SummaryPath { get; private set; }
        public string RequestMRDFPath { get; private set; }
        public string ResponseMRDFPath { get; private set; }
        public string MergedPDFPath { get; private set; }

        public BatchSummary BatchSummary { get; private set; }
        public Client Client { get; private set; }
        public DocType DocType { get; private set; }

        public BatchInfo(BatchSummary batchSummary, Client client)
        {
            if (batchSummary == null || client == null) throw new ArgumentNullException("Batch summary or client cannot be null.");

            BatchSummary = batchSummary;
            Client = client;

            Initialize(batchSummary, client);
        }

        private void Initialize(BatchSummary batchSummary, Client client)
        {
            // TODO: Use the same logic to retrieve app code folder
            //"XYZ-XYZ-123".Substring("".Length, )

            BatchName = batchSummary.JobName.Substring((client.AppCode.Length + 1),
                (batchSummary.JobName.Length - (client.AppCode.Length + 1)));

            //Combined path
            BatchRepo = Path.Combine(Common.PCIRootFolder, client.AppCode, "{0}-{1}".Fill(batchSummary.PrintBatch, client.AppCode));

            PnSRequestPath = Path.Combine(BatchRepo, "PnSRequest");
            PCIRequestPath = Path.Combine(BatchRepo, "PCIRequest");
            PCIResponsePath = Path.Combine(BatchRepo, "PCIResponse");
            PnSResponsePath = Path.Combine(BatchRepo, "PnSResponse");


            RequestXmlPath = Path.Combine(PnSRequestPath, "{0}.xml".Fill(BatchName));

            PnSRequestPDFsPath = Path.Combine(PnSRequestPath, BatchName);

            //Used for storing PCI returned pdf to pns shared path for showing that pdfs to AuthoMail website
            PCIResponseReturnPDFsPath = Path.Combine(Common.PnS_SFT_RootDir, "PCIReturnedPDFs", client.AppCode, batchSummary.PrintBatch.ToString());
        }

        public void InitializeDocTypeMaps(DocType docType, int printBatch, string appCode, string custID)
        {
            DocType = docType;
            BatchZipFilePath = Path.Combine(PCIRequestPath, "{0}-{1}-{2}-{3}.zip".Fill(custID, printBatch, appCode, docType.DocTypeCode.Trim()));

            var pciName = "{0}-{1}-{2}-{3}".Fill(custID, printBatch, appCode, docType.DocTypeCode.Trim());
            SummaryPath = Path.Combine(PCIRequestPath, "{0}.txt".Fill(pciName));
            RequestMRDFPath = Path.Combine(PCIRequestPath, "{0}.dat".Fill(pciName));
            MergedPDFPath = Path.Combine(PCIRequestPath, "{0}.pdf".Fill(pciName));
            ResponseMRDFPath = Path.Combine(PCIResponsePath, "{0}.out".Fill(pciName));
        }

    }

    public class SubmittedBatches
    {
        public int PrintBatchId { get; set; }
        public string PatternBatchCode { get; set; }
        public List<string> ReceivedFileList { get; set; }
    }

    public class MailPieceReceivedOnData
    {
        public int MailPieceId { get; set; }
        public DateTime EffectiveReceivedOnEst { get; set; }
        public Guid UniqueId { get; set; }
        public string MailPieceFileName { get; set; }
    }

    public class DallasBatch
    {
        public string BatchName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
    }

    public class DallasBatchTiming
    {
        public string BatchName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
    }
}
