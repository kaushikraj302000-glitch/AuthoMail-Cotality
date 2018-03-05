using AM.Entities;
using AM.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AM.Console
{
    public class CoreJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CoreJob));
        IClientService clientSvc;
        IBatchService batchSvc;
        IMailPieceService mailPieceSvc;
        ICommonService commonSvc;
        IDocTypeService docTypeSvc;
        public CoreJob()
        {
            clientSvc = new ClientService();
            batchSvc = new BatchService();
            mailPieceSvc = new MailPieceService();
            commonSvc = new CommonService();
            docTypeSvc = new DocTypeService();
        }

        public void ConvertBatchIntoAMXml()
        {
            List<MailBatchDTO> downloadBatches = batchSvc.GetBatchesByStatus(BatchPipelineStatus.Downloaded);

            if (downloadBatches == null)
            {
                log.Debug("Process batches job queue is empty.");
                return;
            }

            foreach (MailBatchDTO batch in downloadBatches)
            {
                Client client = clientSvc.GetClientDetails(batch.ClientId).Single();
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetails(batch.SFTChannelId);

                MailBatchInfo mbi = new MailBatchInfo(batch, client, sftChannelDetails);

                var xmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName, mbi.BatchName + ".xml");

                if (!File.Exists(xmlFilePath))
                {
                    mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                    mbi.MailBatch.ValidationErrors = "No XML file is available in {0} batch.".Fill(mbi.MailBatch.BatchName);

                    batchSvc.UpdateBatchDetails(mbi.MailBatch);

                    commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                    continue;
                }
                MailBatch amParsedData = null;

                try
                {
                    amParsedData = ConvertAMCschemaToAMschema(xmlFilePath, mbi);

                    if (amParsedData != null)
                    {
                        File.WriteAllText(Path.Combine(mbi.AMRequestPath, mbi.BatchName, mbi.BatchName + "_AuthoMail.xml"), XmlUtil.Serialize<MailBatch>(amParsedData));
                    }
                }
                catch (Exception ex)
                {
                    mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                    mbi.MailBatch.ValidationErrors = ex.Message;

                    batchSvc.UpdateBatchDetails(mbi.MailBatch);

                    commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                    continue;
                }

            }
        }

        private MailBatch ConvertAMCschemaToAMschema(string xmlFilePath, MailBatchInfo mbi)
        {
            PackageRequests packageRequest = null;
            try
            {
                XmlDocument batchXML = new XmlDocument();
                batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                packageRequest = XmlUtil.Deserialize<PackageRequests>(batchXML.OuterXml);
            }
            catch (XmlException ex)
            {
                mbi.MailBatch.ValidationStatus = OutComeCodes.Invalid_XML;
                mbi.MailBatch.ValidationErrors = ex.Message;

                batchSvc.UpdateBatchDetails(mbi.MailBatch);

                commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
            }

            MailBatch amParsedBatch = new MailBatch();
            amParsedBatch.ChannelCode = mbi.SFTChannel.Code;
            amParsedBatch.ClientBatchId = mbi.BatchName;
            amParsedBatch.Id = Guid.NewGuid();
            amParsedBatch.Priority = Priority.High;

            List<MailPiece> amParsedPieces = new List<MailPiece>();
            foreach (var package in packageRequest.Packages)
            {
                // If the XML is valid and found any child nodes
                if (package != null)
                {
                    try
                    {
                        if (package != null)
                        {
                            MailPiece amPiece = new MailPiece();
                            amPiece.BusinessFriendlyIdName = "LoanNbr";
                            amPiece.BusinessFriendlyIdValue = package.LoanNbr;
                            amPiece.DocumentOrPackage.Name = package.File;
                            amPiece.DocumentOrPackageType = package.PackageName;
                            amPiece.Id = package.Id;
                            if (package.RecipientInfo != null)
                            {
                                if (package.RecipientInfo.Recipients.Count > 1)
                                {
                                    amPiece.RecipientInfo.Recipient.FirstName = package.RecipientInfo.Recipients[0].FirstName;
                                    amPiece.RecipientInfo.Recipient.LastName = package.RecipientInfo.Recipients[0].LastName;
                                    amPiece.RecipientInfo.Corecipient.FirstName = package.RecipientInfo.Recipients[1].FirstName;
                                    amPiece.RecipientInfo.Corecipient.LastName = package.RecipientInfo.Recipients[1].LastName;
                                }
                                else
                                {
                                    amPiece.RecipientInfo.Recipient.FirstName = package.RecipientInfo.Recipients[0].FirstName;
                                    amPiece.RecipientInfo.Recipient.LastName = package.RecipientInfo.Recipients[0].LastName;
                                }
                                amPiece.RecipientInfo.Recipient.Phone = package.RecipientInfo.RecipientPhoneNo;
                            }

                            if (package.RecipientInfo.RecipientAddress != null)
                            {
                                amPiece.RecipientInfo.Address.AptOrSuiteOrOther = package.RecipientInfo.RecipientAddress.Line2;
                                amPiece.RecipientInfo.Address.Street = package.RecipientInfo.RecipientAddress.Line1;
                                amPiece.RecipientInfo.Address.State = package.RecipientInfo.RecipientAddress.State;
                                amPiece.RecipientInfo.Address.Zip = package.RecipientInfo.RecipientAddress.Zip;
                            }

                            if (package.ReturnInfo != null)
                            {
                                amPiece.ReturnInfo.Recipient.FirstName = package.ReturnInfo.FirstName;
                                amPiece.ReturnInfo.Recipient.LastName = package.ReturnInfo.LastName;
                                amPiece.ReturnInfo.Recipient.Phone = package.ReturnInfo.ReturnPhoneNo;

                                if (package.ReturnInfo.ReturnAddress != null)
                                {
                                    amPiece.ReturnInfo.Address.AptOrSuiteOrOther = package.RecipientInfo.RecipientAddress.Line2;
                                    amPiece.ReturnInfo.Address.Street = package.RecipientInfo.RecipientAddress.Line1;
                                    amPiece.ReturnInfo.Address.State = package.RecipientInfo.RecipientAddress.State;
                                    amPiece.ReturnInfo.Address.Zip = package.RecipientInfo.RecipientAddress.Zip;
                                }
                            }

                            amPiece.CustomInfo = new List<Metadata>()
                            {
                                new Metadata() { Key = "ReceivedDate", Value = package.ReceivedDate.HasValue?package.ReceivedDate.Value.ToString():"" },
                                new Metadata() { Key = "RequestId", Value = package.RequestId},
                                new Metadata() { Key = "ShippingCredCode", Value = package.ShippingCredCode },
                                new Metadata() { Key = "ShippingMethod", Value = package.ShippingMethod.ToString() },
                                new Metadata() { Key = "CustomerIdentifier", Value = package.CustomerIdentifier }
                            };

                            amParsedPieces.Add(amPiece);
                        }
                    }
                    catch (Exception)
                    {
                        //TODO : Need to consult with Ravi Sir 
                        throw;
                    }
                    amParsedBatch.MailPieces = amParsedPieces;

                    return amParsedBatch;
                }
            }

            return null;
        }
    }
}
