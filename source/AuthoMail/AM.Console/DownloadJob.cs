using AM.Entities;
using AM.Services;
using log4net;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace AM.Console
{
    public class DownloadJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DownloadJob));

        IClientService clientSvc;
        IBatchService batchSvc;
        IMailPieceService mailPieceSvc;
        ICommonService commonSvc;
        public DownloadJob()
        {
            clientSvc = new ClientService();
            batchSvc = new BatchService();
            mailPieceSvc = new MailPieceService();
            commonSvc = new CommonService();
        }

        public void DownloadNewBatch()
        {
            log.Debug("Download Batch Process Started");

            Client client = clientSvc.GetClientDetails(null).Single(x => x.ClientName == "USAA");

            ClientSFTChannel channelMap = clientSvc.GetClientSFTChannelMapDetails(client.Id).SingleOrDefault();

            if (channelMap != null)
            {
                SFTChannel channelDetails = clientSvc.GetSFTChannelDetails(channelMap.SFTChannelId);
                if (channelDetails != null)
                {
                    SFTUtil sftSite = null;
                    try
                    {
                        sftSite = new SFTUtil(Common.GetProtocolType(channelDetails.Protocol), channelDetails.Host, channelDetails.UserName, channelDetails.Password, 300000);
                        sftSite.Connect();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }

                    if (sftSite.IsDownloadDirectoryAvailable(channelDetails.ADERequestDir))
                    {

                    }
                }
            }

            //if (sftSite.IsDownloadDirectoryAvailable(client.SFTADERequestDir))
            //{
            //    var listFiles = sftSite.ListDirectory(client.SFTADERequestDir).Where(x => Path.GetExtension(x).Equals(".xml", StringComparison.CurrentCultureIgnoreCase)).ToList();
            //    if (listFiles != null && listFiles.Count > 0)
            //    {
            //        log.Debug(listFiles.Count.ToString() + " files found for client : " + client.AppCode);

            //        foreach (var file in listFiles)
            //        {
            //            string localDownloadPath = Path.Combine(Common.AuthoMailRootFolder, client.AppCode, Path.GetFileNameWithoutExtension(file));
            //            string serverFiledownloadPath = Path.Combine(client.SFTADERequestDir, file);
            //            string serverArchiveFilePath = Path.Combine(client.SFTADERequestArchiveDir, file);

            //            //Check if directory exist or not, if exist then delete and recreate
            //            if (Directory.Exists(localDownloadPath))
            //            {
            //                Directory.Delete(localDownloadPath, true);
            //                Directory.CreateDirectory(localDownloadPath);
            //            }
            //            else
            //                Directory.CreateDirectory(localDownloadPath);

            //            Batch mailBatch = new Batch();
            //            mailBatch.ClientId = client.Id;

            //            sftSite.DownloadFile(Path.Combine(localDownloadPath, file), serverFiledownloadPath);

            //            mailBatch.ReceivedOn = sftSite.LastModifiedTimeOfFile(serverFiledownloadPath);
            //            mailBatch.BatchFilePath = Path.Combine(localDownloadPath, file);

            //            sftSite.MoveFile(serverFiledownloadPath, serverArchiveFilePath);

            //            ProcessXML(sftSite, Path.Combine(localDownloadPath, file), mailBatch, client, localDownloadPath);

            //        }
            //    }
            //    else
            //    {
            //        MailUtility.SendNoBatchReceivedNotificationEmail(Common.UtcToClientTimeZone(Common.GetCurrentTime(), client.TimeZone));
            //    }
            //}
            //else
            //{
            //    //TODO: Mail for Path is not available to client or internal team
            //}
        }

        private void ProcessXML(SFTUtil sftSite, string xmlFilePath, Batch mailBatch, Client client, string localDownloadPath, SFTChannel sftChannel)
        {
            string batchName = Path.GetFileNameWithoutExtension(xmlFilePath);
            //ValidationResult validationResult = XmlUtil.ValidateXml(Common.schemas, xmlFilePath);
            //if (!validationResult.HasErrors)
            //{

            MailBatch processXML = XmlUtil.Deserialize<MailBatch>(File.ReadAllText(xmlFilePath));
            if (processXML != null)
            {
                mailBatch.Id = processXML.Id;
                mailBatch.Priority = processXML.Priority.GetStringValue();
                mailBatch.DocumentOrPackageType = processXML.DocumentOrPackageType;
                //mailBatch.BusinessFriendlyIdName = processXML.BusinessFriendlyIdName != null ? processXML.BusinessFriendlyIdName : "";
                //mailBatch.BusinessFriendlyIdValue = processXML.BusinessFriendlyIdValue != null ? processXML.BusinessFriendlyIdValue : "";
                mailBatch.BatchName = batchName;
                if (processXML.ReturnInfo != null)
                {
                    mailBatch.IsReturnInfoSpecified = true;

                    if (processXML.ReturnInfo.Address != null)
                    {
                        mailBatch.RetRcptAddressAptOrSuiteOrOther = processXML.ReturnInfo.Address.AptOrSuiteOrOther;
                        mailBatch.RetRcptAddressCity = processXML.ReturnInfo.Address.City;
                        mailBatch.RetRcptAddressState = processXML.ReturnInfo.Address.State;
                        mailBatch.RetRcptAddressStreet = processXML.ReturnInfo.Address.Street;
                        mailBatch.RetRcptAddressZIP = processXML.ReturnInfo.Address.Zip;
                        mailBatch.RetRcptAddrOptionalLine1AboveStreet = processXML.ReturnInfo.Address.OptionalLine1AboveStreet;
                        mailBatch.RetRcptAddrOptionalLine2AboveStreet = processXML.ReturnInfo.Address.OptionalLine2AboveStreet;
                        mailBatch.RetRcptAddrOptionalLine1BelowStreet = processXML.ReturnInfo.Address.OptionalLine1BelowStreet;
                        mailBatch.RetRcptAddrOptionalLine2BelowStreet = processXML.ReturnInfo.Address.OptionalLine2BelowStreet;
                    }

                    if (processXML.ReturnInfo.Recipient != null)
                    {
                        mailBatch.RetRcptFirstName = processXML.ReturnInfo.Recipient.FirstName;
                        mailBatch.RetRcptLastName = processXML.ReturnInfo.Recipient.LastName;
                        mailBatch.RetRcptMiddleName = processXML.ReturnInfo.Recipient.MiddleName;
                        mailBatch.RetRcptOrganization = processXML.ReturnInfo.Recipient.Organization;
                        mailBatch.RetRcptPhone = processXML.ReturnInfo.Recipient.Phone;
                        mailBatch.RetRcptPrefix = processXML.ReturnInfo.Recipient.Prefix;
                        mailBatch.RetRcptSuffix = processXML.ReturnInfo.Recipient.Suffix;
                    }
                }

                mailBatch.BatchClustId = batchSvc.InsertBatchDetails(mailBatch);

                if (processXML.MailPieces != null && processXML.MailPieces.Count > 0)
                {
                    List<MailPieceDTO> amMailPieces = new List<MailPieceDTO>();
                    Dictionary<string, string> archiveFileList = new Dictionary<string, string>();
                    foreach (MailPiece mailPiece in processXML.MailPieces)
                    {
                        MailPieceDTO amMailPiece = new MailPieceDTO();
                        amMailPiece.Id = mailPiece.Id;
                        amMailPiece.BatchClustId = mailBatch.BatchClustId;
                        amMailPiece.BusinessFriendlyIdName = mailPiece.BusinessFriendlyIdName;
                        amMailPiece.BusinessFriendlyIdValue = mailPiece.BusinessFriendlyIdValue;
                        amMailPiece.DocumentOrPackageType = mailPiece.DocumentOrPackageType;

                        if (mailPiece.ReturnInfo != null)
                        {
                            amMailPiece.IsReturnInfoSpecified = true;

                            if (mailPiece.ReturnInfo.Address != null)
                            {
                                amMailPiece.RetRcptAddressAptOrSuiteOrOther = mailPiece.ReturnInfo.Address.AptOrSuiteOrOther;
                                amMailPiece.RetRcptAddressCity = mailPiece.ReturnInfo.Address.City;
                                amMailPiece.RetRcptAddressState = mailPiece.ReturnInfo.Address.State;
                                amMailPiece.RetRcptAddressStreet = mailPiece.ReturnInfo.Address.Street;
                                amMailPiece.RetRcptAddressZIP = mailPiece.ReturnInfo.Address.Zip;
                                amMailPiece.RetRcptAddrOptionalLine1AboveStreet = mailPiece.ReturnInfo.Address.OptionalLine1AboveStreet;
                                amMailPiece.RetRcptAddrOptionalLine2AboveStreet = mailPiece.ReturnInfo.Address.OptionalLine2AboveStreet;
                                amMailPiece.RetRcptAddrOptionalLine1BelowStreet = mailPiece.ReturnInfo.Address.OptionalLine1BelowStreet;
                                amMailPiece.RetRcptAddrOptionalLine2BelowStreet = mailPiece.ReturnInfo.Address.OptionalLine2BelowStreet;
                            }

                            if (mailPiece.ReturnInfo.Recipient != null)
                            {
                                amMailPiece.RetRcptFirstName = mailPiece.ReturnInfo.Recipient.FirstName;
                                amMailPiece.RetRcptLastName = mailPiece.ReturnInfo.Recipient.LastName;
                                amMailPiece.RetRcptMiddleName = mailPiece.ReturnInfo.Recipient.MiddleName;
                                amMailPiece.RetRcptOrganization = mailPiece.ReturnInfo.Recipient.Organization;
                                amMailPiece.RetRcptPhone = mailPiece.ReturnInfo.Recipient.Phone;
                                amMailPiece.RetRcptPrefix = mailPiece.ReturnInfo.Recipient.Prefix;
                                amMailPiece.RetRcptSuffix = mailPiece.ReturnInfo.Recipient.Suffix;
                            }
                        }

                        string validationOutCome = ValidationClientWise(client.AppCode, mailPiece);

                        if (validationOutCome == OutComeCodes.No_Error)
                        {
                            amMailPiece.IsRcptInfoSpecified = true;
                            amMailPiece.RcptFirstName = mailPiece.RecipientInfo.Recipient.FirstName;
                            amMailPiece.RcptLastName = mailPiece.RecipientInfo.Recipient.LastName;
                            amMailPiece.RcptMiddleName = mailPiece.RecipientInfo.Recipient.MiddleName;
                            amMailPiece.RcptOrganization = mailPiece.RecipientInfo.Recipient.Organization;
                            amMailPiece.RcptPhone = mailPiece.RecipientInfo.Recipient.Phone;
                            amMailPiece.RcptPrefix = mailPiece.RecipientInfo.Recipient.Prefix;
                            amMailPiece.RcptSuffix = mailPiece.RecipientInfo.Recipient.Suffix;

                            if (mailPiece.RecipientInfo.Corecipient != null)
                            {
                                amMailPiece.CoRcptFirstName = mailPiece.RecipientInfo.Corecipient.FirstName;
                                amMailPiece.CoRcptLastName = mailPiece.RecipientInfo.Corecipient.LastName;
                                amMailPiece.CoRcptMiddleName = mailPiece.RecipientInfo.Corecipient.MiddleName;
                                amMailPiece.CoRcptOrganization = mailPiece.RecipientInfo.Corecipient.Organization;
                                amMailPiece.CoRcptPhone = mailPiece.RecipientInfo.Corecipient.Phone;
                                amMailPiece.CoRcptPrefix = mailPiece.RecipientInfo.Corecipient.Prefix;
                                amMailPiece.CoRcptSuffix = mailPiece.RecipientInfo.Corecipient.Suffix;
                            }

                            if (mailPiece.RecipientInfo.Address != null)
                            {
                                amMailPiece.RcptAddressAptOrSuiteOrOther = mailPiece.RecipientInfo.Address.AptOrSuiteOrOther;
                                amMailPiece.RcptAddressCity = mailPiece.RecipientInfo.Address.City;
                                amMailPiece.RcptAddressState = mailPiece.RecipientInfo.Address.State;
                                amMailPiece.RcptAddressStreet = mailPiece.RecipientInfo.Address.Street;
                                amMailPiece.RcptAddressZIP = mailPiece.RecipientInfo.Address.Zip;
                                amMailPiece.RcptAddrOptionalLine1AboveStreet = mailPiece.RecipientInfo.Address.OptionalLine1AboveStreet;
                                amMailPiece.RcptAddrOptionalLine2AboveStreet = mailPiece.RecipientInfo.Address.OptionalLine2AboveStreet;
                                amMailPiece.RcptAddrOptionalLine1BelowStreet = mailPiece.RecipientInfo.Address.OptionalLine1BelowStreet;
                                amMailPiece.RcptAddrOptionalLine2BelowStreet = mailPiece.RecipientInfo.Address.OptionalLine2BelowStreet;
                            }

                            List<string> availablePDF = sftSite.ListDirectory(sftChannel.ADERequestDir).Where(x => Path.GetExtension(x).Equals(".pdf", StringComparison.CurrentCultureIgnoreCase)).ToList();
                            string pdfLocalFilePath = string.Empty;
                            string pdfServerFilePath = string.Empty;
                            string pdfServerArchiveFilePath = string.Empty;
                            if (availablePDF.Count > 0)
                            {
                                if (mailPiece.DocumentOrPackage.Name != null || string.IsNullOrEmpty(mailPiece.DocumentOrPackage.Name))
                                {
                                    amMailPiece.FileName = mailPiece.DocumentOrPackage.Name;

                                    if (availablePDF.Contains(mailPiece.DocumentOrPackage.Name))
                                    {
                                        pdfLocalFilePath = Path.Combine(localDownloadPath, mailPiece.DocumentOrPackage.Name);
                                        pdfServerFilePath = Path.Combine(sftChannel.ADERequestDir, mailPiece.DocumentOrPackage.Name);
                                        pdfServerArchiveFilePath = Path.Combine(sftChannel.ADERequestArchiveDir, mailPiece.DocumentOrPackage.Name);
                                        if (!archiveFileList.ContainsKey(pdfServerFilePath))
                                            archiveFileList.Add(pdfServerFilePath, pdfServerArchiveFilePath);

                                        sftSite.DownloadFile(pdfLocalFilePath, pdfServerFilePath);

                                        amMailPiece.DownloadedFilePath = pdfLocalFilePath;
                                        amMailPiece.ErrorCode = OutComeCodes.No_Error.ToString();
                                    }
                                    else
                                    {
                                        amMailPiece.ErrorCode = OutComeCodes.Missing_PDF.ToString();
                                        amMailPiece.ErrorDesc = "Pdf file is missing on server.";
                                    }
                                }
                                else
                                {
                                    amMailPiece.FilePattern = mailPiece.DocumentOrPackage.Pattern;

                                    if (availablePDF.Contains(mailPiece.DocumentOrPackage.Pattern))
                                    {
                                        string pdfFileName = availablePDF.Single(x => x.Contains(mailPiece.DocumentOrPackage.Pattern));

                                        pdfLocalFilePath = Path.Combine(localDownloadPath, pdfFileName);
                                        pdfServerFilePath = Path.Combine(sftChannel.ADERequestDir, pdfFileName);
                                        pdfServerArchiveFilePath = Path.Combine(sftChannel.ADERequestArchiveDir, pdfFileName);

                                        sftSite.DownloadFile(pdfLocalFilePath, pdfServerFilePath);

                                        if (!archiveFileList.ContainsKey(pdfServerFilePath))
                                            archiveFileList.Add(pdfServerFilePath, pdfServerArchiveFilePath);

                                        amMailPiece.DownloadedFilePath = pdfLocalFilePath;
                                        amMailPiece.ErrorCode = OutComeCodes.No_Error.ToString();
                                    }
                                    else
                                    {
                                        amMailPiece.ErrorCode = OutComeCodes.Missing_PDF.ToString();
                                        amMailPiece.ErrorDesc = "Pdf file is missing on server.";
                                    }
                                }
                            }
                        }
                        else
                        {
                            amMailPiece.ErrorCode = validationOutCome.ToString();
                            amMailPiece.ErrorDesc = "";
                        }
                        amMailPieces.Add(amMailPiece);
                    }
                    //mailPieceSvc.InsertBatchMailPieceDetails(amMailPieces);

                    #region Archive Files After Download
                    foreach (var archiveFile in archiveFileList)
                    {
                        sftSite.MoveFile(archiveFile.Key, archiveFile.Value);
                    }
                    #endregion
                    commonSvc.UpdateBatchPipeLineStatus(mailBatch.BatchClustId, BatchPipelineStatus.Downloaded, Common.GetCurrentTime());

                }
            }
            else
            {
                //TODO:Mail if found null
            }
            //}
            //else
            //{
            //    //TODO:Mail if found any XML level error
            //}
        }

        private string ValidationClientWise(string clientCode, MailPiece mailPiece)
        {
            string validationResultClientWise = OutComeCodes.No_Error;
            switch (clientCode)
            {
                case "USAA": validationResultClientWise = mailPiece.RecipientInfo.Recipient == null ? OutComeCodes.Recipient_Name_Missing_Or_Invalid : OutComeCodes.No_Error;
                    break;
                default:
                    validationResultClientWise = OutComeCodes.No_Error;
                    break;
            }
            return validationResultClientWise;
        }
    }
}
