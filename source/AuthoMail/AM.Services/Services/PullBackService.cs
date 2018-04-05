using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Lib;
using System.Data;
using System.Net.Mail;
using System.IO;
using System.Web.Hosting;
namespace AM.Services
{
    public interface IPullBackService : IDisposable
    {
        List<PullBackViewModel> GetMailPieceData(List<string> mpIds);
        List<PullBackRequestForm> GetPullBackData(List<string> mpIds);
        int UpdatePullBackRequestedOn(string mpId, string userId);
        int InsertPullBackRequestData(string mpId, int clientId);
        List<MailPieces> GetPullBackRequestList(string businessFriendlyIdName, string businessFriendlyIdValue, string docType, DateTime startDate, DateTime endDate, int clientId);
        List<QuickPullBackResponse> GetQuickPullBackResponseFromBusinessFriendlyValue(string businessFriendlyIdName, string businessFriendlyIdValue, int clientId);
        List<QuickPullBackResponse> GetQuickPullBackResponse(List<string> mailPieceIds, int clientId);
        bool GeneratePullBackForm(List<PullBackRequestForm> pullBackData);
    }
    public class PullBackService : ServiceBase, IPullBackService
    {
        public List<PullBackViewModel> GetMailPieceData(List<string> mpIds)
        {
            string qry = string.Format("SELECT AMM.MailBatchId,PM.[UniqueId] as MailPieceUniqueId,PB.PrintBatch,PB.PipelineStatus,PM.MailPieceStatus,AMM.IsPullBackRequested,AMM.PullBackRequestedOn,AMM.PulledBackRequestedBy,PU.UserName FROM PnS.MRDFRequest PM INNER JOIN PnS.BatchSummary PB ON PM.JobId = PB.PrintBatch INNER JOIN AM.MailPieces AMM ON PM.[UniqueId] = AMM.UniqueId LEFT OUTER JOIN [Profile].Users PU ON PU.Id = AMM.PulledBackRequestedBy WHERE PM.[UniqueId] IN @mailPieceIds");
            var data = db.Query<PullBackViewModel>(qry, new { mailPieceIds = mpIds }).ToList();
            return data;
        }
        public List<PullBackRequestForm> GetPullBackData(List<string> mpIds)
        {
            string qry = string.Format(";WITH batchSummary AS (SELECT PrintFileName,SubmittedOn,ClientId,PrintBatch,DocTypeId  FROM PnS.BatchSummary),docTypes AS (SELECT Id,DocTypeCode FROM AM.DocTypes WHERE IsActive = 1),mrdfReq AS(SELECT JobId,PieceId,[UniqueId] FROM PnS.MRDFRequest),Client AS(SELECT Id,Name FROM Common.Clients),RESULT AS(SELECT PBS.PrintFileName,PBS.SubmittedOn,PMR.JobId,PMR.PieceId,CC.Name,AD.DocTypeCode FROM batchSummary PBS	INNER JOIN AM.DocTypes AD ON AD.Id = PBS.DocTypeId	INNER JOIN mrdfReq PMR ON PBS.PrintBatch = PMR.JobId INNER JOIN Client CC ON PBS.ClientId = CC.Id WHERE PMR.[UniqueId] IN @mailPieceIds)(SELECT PrintFileName,SubmittedOn,JobId AS PrintBatch,PieceId = STUFF((SELECT DISTINCT ','+ CAST(pRES.PieceId AS VARCHAR(255)) FROM RESULT pRES WHERE pRES.JobId = RES.JobId FOR XML PATH('')),1,1,''),Name,DocTypeCode FROM RESULT RES GROUP BY RES.JobId,PrintFileName,SubmittedOn,Name,DocTypeCode)");

            var data = db.Query<PullBackRequestForm>(qry, new { mailPieceIds = mpIds }).ToList();
            return data;
        }

        public int UpdatePullBackRequestedOn(string mpId, string userId)
        {
            string qry = string.Format(@"UPDATE AM.MailPieces SET PullBackRequestedOn = @utcDate,PulledBackRequestedBy = @UserId WHERE UniqueId = @id");
            var data = db.Execute(qry, new { utcDate = DateTime.UtcNow, UserId = string.IsNullOrEmpty(userId) ? DBNull.Value.ToString() : userId, id = mpId });
            return data;
        }

        public int InsertPullBackRequestData(string mpId, int clientId)
        {
            return db.Execute("[dbo].[usp_InsertPullBackRequestsData]", new { @MailPieceUniqueId = mpId, @ClientId = clientId }, commandType: CommandType.StoredProcedure);
        }

        public List<MailPieces> GetPullBackRequestList(string businessFriendlyIdName, string businessFriendlyIdValue, string docType, DateTime startDate, DateTime endDate, int clientId)
        {
            string query = "select * from [AM].[MailBatches] b, [AM].[DocTypes] d,[Common].[Clients] c,[AM].[MailPieces] m left outer join [Profile].[Users] u on m.[PulledBackRequestedBy]=u.Id where ((m.MailBatchId=b.Id and m.ClientId=c.Id and d.Id=m.DocTypeId) and b.ClientId=@clientId)  AND ISNULL(m.BusinessFriendlyIdName, '') = ISNULL(@businessFriendlyIdName, ISNULL(m.BusinessFriendlyIdName, '')) AND ISNULL(m.BusinessFriendlyIdValue, '') = ISNULL(@businessFriendlyIdValue, ISNULL(m.BusinessFriendlyIdValue, '')) AND ISNULL(m.DocTypeId, '') = ISNULL(@docType, ISNULL(m.DocTypeId, '')) AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDate, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDate, ISNULL(b.ReceivedOn, 0))";

            var data = db.Query<MailBatches, Clients, MailPieces, Users, DocTypes, MailPieces>(query,
               (b, c, m, u, d) =>
               {
                   m.Clients = c;
                   m.MailBatches = b;
                   m.DocTypes = d;
                   m.Users = u;
                   return m;
               },
                new
                {
                    clientId = clientId,
                    businessFriendlyIdName = businessFriendlyIdName.IsNullOrWhiteSpace() ? null : businessFriendlyIdName,
                    businessFriendlyIdValue = businessFriendlyIdValue.IsNullOrWhiteSpace() ? null : businessFriendlyIdValue,
                    startDate = startDate.ToStartDateTime().EstToUtc(),
                    endDate = endDate.ToEndDateTime().EstToUtc(),
                    docType = docType.IsNullOrWhiteSpace() ? null : docType,
                }
                ).ToList();

            //Dictionary<string, object> parameters = new Dictionary<string, object>();
            //parameters.Add("@startDate", startDate.ToStartDateTime().EstToUtc());
            //parameters.Add("@endDate", endDate.ToEndDateTime().EstToUtc());
            //parameters.Add("@clientId", clientId);
            //if (!businessFriendlyIdName.IsNullOrWhiteSpace() && !businessFriendlyIdValue.IsNullOrWhiteSpace())
            //{
            //    query = string.Format("{0} AND (m.[BusinessFriendlyIdName]=@businessFriendlyIdName AND m.BusinessFriendlyIdValue=@businessFriendlyIdValue)", query);
            //    parameters.Add("@businessFriendlyIdName", businessFriendlyIdName);
            //    parameters.Add("@businessFriendlyIdValue", businessFriendlyIdValue);
            //}
            //if (!docType.IsNullOrWhiteSpace())
            //{
            //    query = string.Format("{0} AND (m.DocTypeId=@docType)", query);
            //    parameters.Add("@docType", docType);
            //}

            return data;
        }
        public List<QuickPullBackResponse> GetQuickPullBackResponseFromBusinessFriendlyValue(string businessFriendlyIdName, string businessFriendlyIdValue, int clientId)
        {
            string query = PullBackServiceQuery.GetQuickPullBackResponseFromBusinessFriendlyValue;
            var mailPieces = db.Query<QuickPullBackResponse>(query, new { businessFriendlyIdName = businessFriendlyIdName, businessFriendlyIdValue = businessFriendlyIdValue, clientId = clientId }).ToList();
            return mailPieces;
        }
        public List<QuickPullBackResponse> GetQuickPullBackResponse(List<string> mailPieceIds, int clientId)
        {
            string query = PullBackServiceQuery.GetQuickPullBackResponse;
            var response = db.Query<QuickPullBackResponse>(query, new { clientId = clientId, mailPieceIds = mailPieceIds }).ToList();
            return response;
        }

        public bool GeneratePullBackForm(List<PullBackRequestForm> pullBackData)
        {
            bool isGenerated = true;
            try
            {
                foreach (var item in pullBackData)
                {
                    Dictionary<string, string> dataForPullBackForm = new Dictionary<string, string>();
                    DateTime eastern = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, Config.API_PB_ClientTimeZone);
                    dataForPullBackForm.Add("RequestDateTime", eastern.ToString());
                    dataForPullBackForm.Add("FileName", string.Format("{0} {1}", item.PrintFileName, item.SubmittedOn.Value.ToString("MM/dd/yy")));
                    dataForPullBackForm.Add("JobNumber", item.PrintBatch.ToString());
                    dataForPullBackForm.Add("PackagesToPull", string.Format("{0} : {1}", item.PrintBatch.ToString(), item.PieceId.ToString()));
                    dataForPullBackForm.Add("Coordinator", string.Format("AuthoMail, ADE, {0}-{1}", item.Name, item.DocTypeCode));
                    dataForPullBackForm.Add("PkgInstruction", string.Empty);
                    dataForPullBackForm.Add("Comments", string.Empty);

                    byte[] output = null;
                    string attachmentPdfName = string.Format("{0}_{1}.pdf", item.PrintBatch.ToString(), DateTime.Now.ToString("ddMMyyyyhhmmss"));
                    //string outputPdfPath = Path.Combine(Config.API_PB_OutputDir, string.Format("{0}_{1}.pdf", item.PrintBatch.ToString(), DateTime.Now.ToString("ddMMyyyyhhmmss")));
                    //Helper.MailMerge(Config.API_PB_PullBackFormTemplatePath, dataForPullBackForm, outputPdfPath);
                    Helper.MailMerge(Config.API_PB_PullBackFormTemplatePath, dataForPullBackForm, ref output);
                    bool isEmailSent = SendEmailWithPullBackForm(output, attachmentPdfName);

                }
            }
            catch (Exception ex)
            {
                isGenerated = false;
                throw ex;
            }
            return isGenerated;
        }

        private bool SendEmailWithPullBackForm(byte[] attachmentPdf, string attachmentPdfName)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.To.Add(Config.API_Email_To);
                    msg.CC.Add(Config.API_Email_Cc);
                    msg.Subject = Config.API_Email_Subject;
                    msg.Priority = MailPriority.High;
                    msg.Body = string.Format("{0} {1}.", Config.API_Email_BodyMsg, attachmentPdfName);
                    msg.Attachments.Add(new Attachment(new MemoryStream(attachmentPdf), attachmentPdfName));
                    SmtpClient sc = new SmtpClient();
                    sc.Send(msg);
                    isEmailSent = true;
                }

            }
            catch (Exception err)
            {
                throw err;
            }

            return isEmailSent;
        }
    }

    public static class PullBackServiceQuery
    {

        public static string GetQuickPullBackResponseFromBusinessFriendlyValue = "select b.ClientBatchId,c.TimeZone as [ClientTimeZone], m.Id as [MailPieceId], m.UniqueId as [MailPieceUniqueId],b.Id as [BatchId],b.UniqueId as [BatchUniqueId],b.BatchName,b.ReceivedOn,m.BusinessFriendlyIdName,m.BusinessFriendlyIdValue,m.[FileName],m.[ShippingStatus],m.IsPullBackRequested from [AM].[MailBatches] b,[AM].[MailPieces] m, [Common].[Clients] c where b.Id = m.MailBatchId and m.BusinessFriendlyIdName=@businessFriendlyIdName and m.ClientId=c.Id  m.BusinessFriendlyIdValue=@businessFriendlyIdValue AND b.ClientId=@clientId";
        public static string GetQuickPullBackResponse = "select b.ClientBatchId,c.TimeZone as [ClientTimeZone], m.Id as [MailPieceId], m.UniqueId as [MailPieceUniqueId],b.Id as [BatchId],b.UniqueId as [BatchUniqueId],b.BatchName,b.ReceivedOn,m.BusinessFriendlyIdName,m.BusinessFriendlyIdValue,m.[FileName]m.ShippingStatus,m.IsPullBackRequested from [AM].[MailBatches] b,[AM].[MailPieces] m, [Common].[Clients] c where b.Id =m.MailBatchId and m.ClientId=c.Id and m.UniqueId=@mailPieceIds AND b.ClientId=@clientId";
    }
}
