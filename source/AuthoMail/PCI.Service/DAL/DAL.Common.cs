using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Service
{
    partial class DAL
    {
        private static Database db = DatabaseFactory.CreateDatabase();

        public static List<int> GetNextBatchIdByJobName(string jobName, string operation)
        {
            var batchIds = db.ExecuteDataSet("usp_GetNextBatchIdByJobName", jobName, operation);

            if (batchIds != null && batchIds.Tables.Count > 0)
                return batchIds.Tables[0].AsEnumerable().Select(id => id.Field<int>("BatchId")).ToList();

            return null;
        }

        public static BatchSummary GetBatchSummaryById(int batchId)
        {
            var ds = GetBatchSummaryDataById(batchId);
            return ds.DataTableAccessor<BatchSummary>(false).FirstOrDefault();
        }

        public static DataSet GetBatchSummaryDataById(int batchId)
        {
            return db.ExecuteDataSet("usp_GetBatchSummaryById", batchId);
        }

        public static Client GetClientAppDetailsById(int appId)
        {
            var accessor = db.CreateSprocAccessor<Client>("usp_GetClientAppDetailsById");
            return accessor.Execute(appId).FirstOrDefault();
        }

        public static List<DocType> GetDocTypeDetailsByAppId(int appId)
        {
            var accessor = db.CreateSprocAccessor<DocType>("usp_GetDocTypeDetailsByAppId");
            return accessor.Execute(appId).ToList();
        }

        public static int UpdateBatchPipeLineStatus(int batchId, string pipeLineStatus, DateTime timestamp)
        {
            return db.ExecuteSqlNonQuery("UPDATE [PnS].[BatchSummary] SET PipelineStatus = @p1, {0}On = @p2 WHERE PrintBatch = @p3".Fill(pipeLineStatus), pipeLineStatus, timestamp, batchId);
        }

        public static int UpdateBatchStatus(int batchId, string batchStatus)
        {
            return db.ExecuteSqlNonQuery("UPDATE [PnS].[BatchSummary] SET BatchStatus = @p1 WHERE PrintBatch = @p2", batchStatus, batchId);
        }

        public static int UpdateFileReceivedStatus(int jobId, int pieceId)
        {
            return db.ExecuteSqlNonQuery("UPDATE [PnS].[MRDFRequest] SET IsPDFReceived = 1 WHERE [JobId] = @p1 AND [PieceId] = @p2", jobId, pieceId);
        }

        public static int UpdateFileReceivedBatchStatus(int batchId, DateTime timestamp)
        {
            return db.ExecuteSqlNonQuery("UPDATE [PnS].[BatchSummary] SET PDFsReceivedOn = @p1 WHERE PrintBatch = @p2", timestamp, batchId);
        }

        public static void LogBatchTimeLine(int batchId, string activityName, DateTime timeStamp)
        {
            db.ExecuteNonQuery("usp_LogBatchTimeLine", batchId, activityName, timeStamp);
        }

        public static string GetSystemSettingsValue(string keyId)
        {
            return db.ExecuteScalar("usp_GetSystemSettingsById", keyId).ToString();
        }

        public static List<DallasBatch> GetDallasBatches(DateTime estToday)
        {
            var dallasTimings = db.ExecuteDataSet(CommandType.Text, "Select * from dbo.ufn_GetDallasBatches('" + estToday.ToString() + "')");
            return dallasTimings.DataTableAccessor<DallasBatch>(false).ToList();
        }

        public static List<DallasBatchTiming> GetDallasBatchTimings(DateTime estToday)
        {
            var dallasTimings = db.ExecuteDataSet("usp_GetDallasBatchTimingsPnS", estToday);
            return dallasTimings.DataTableAccessor<DallasBatchTiming>(false).ToList();
        }

        public static int UpdatePrintTrailBatchStatus(int printTrailBatchId, string pipeLineStatus, DateTime timestamp)
        {
            return db.ExecuteSqlNonQuery("UPDATE [PnS].[PrintTrailBatches] SET {0}On = @p1 WHERE Id = @p2".Fill(pipeLineStatus), timestamp, printTrailBatchId);
        }

        public static PrintTrailDetail GetPrintTrailDetailByPrintTrailId(int printTrailId)
        {
            var printTrailDetail = db.ExecuteDataSet("usp_GetPrintTrailDetailByPrintTrailId", printTrailId);
            return printTrailDetail.DataTableAccessor<PrintTrailDetail>(false).Single();
        }

        public static PrintTrailDetail GetPrintTrailDetailByPrintTrailName(string printTrailName)
        {
            var printTrailDetail = db.ExecuteDataSet("usp_GetPrintTrailDetailByPrintTrailName", printTrailName);
            return printTrailDetail.DataTableAccessor<PrintTrailDetail>(false).Single();
        }

        public static int GetMailBatchesReceivedCount(DateTime startDateTime, DateTime endDateTime, string status)
        {
            string query = @"SELECT COUNT(*) FROM [AM].[PrintBatches] PB LEFT OUTER JOIN AM.MailBatches B ON PB.MailBatchId = B.Id LEFT OUTER JOIN PnS.BatchSummary BS ON PB.UniqueId = BS.BatchIdentifier WHERE B.ReceivedOn >=@p1 AND B.ReceivedOn <=@p2";

            if (status == "Received")
                query += " AND B.IsReceived=1";
            else
                query += " AND (B.IsPrepped = 1 OR B.IsReportPrepped = 1 OR B.IsReportedBack = 1) AND (BS.IsPrepped = 1 OR BS.IsReportPrepped = 1 OR BS.IsReportedBack = 1)";

            return db.ExecuteSqlScalar<int>(query, startDateTime, endDateTime);
        }
    }
}
