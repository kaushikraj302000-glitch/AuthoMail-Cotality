using Microsoft.Practices.EnterpriseLibrary.Data;
using PCI.Lib;
using PCI.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PCI.Service
{
    partial class DAL
    {
        public static int UpdatePnSRequestDataInBatchSummary(BatchSummary batchSummary)
        {
            DateTime? dueDate = null;
            int? slaDays = null;

            if (batchSummary.DueDate != DateTime.MinValue)
            {
                dueDate = batchSummary.DueDate;
                slaDays = batchSummary.SLADays;
            }

            return db.ExecuteSqlNonQuery(@"UPDATE [PnS].[BatchSummary] SET ValidationStatus=@p1, OnValidationErrors=@p2, ValidationErrors=@p3, JulianDate=@p4, Images=@p5, dueDate=@p6,  slaDays=@p7, BatchIdentifier=@p8, PrintTrailId=@p9, PrintFileName=@p10,PrintLocation=@p11, DocTypeId=@p12  WHERE PrintBatch = @p13", batchSummary.ValidationStatus, batchSummary.OnValidationErrors, batchSummary.ValidationErrors, batchSummary.JulianDate, batchSummary.Images, dueDate, slaDays, batchSummary.BatchIdentifier, batchSummary.PrintTrailId, batchSummary.PrintFileName, batchSummary.PrintLocation, batchSummary.DocTypeId, batchSummary.PrintBatch);
        }

        public static void UpdateBatchSummaryData(List<PCIField> batchSummarySchema, BatchSummary batchSummary)
        {
            var usedColumns = new[] { "PrintFileName", "JulianDate", "Images", "DueDate", "SLADays", "ValidationStatus", "OnValidationErrors", "ValidationErrors", "BatchIdentifier", "PrintTrailId", "PrintLocation", "DocTypeId" };

            var skippedColumns = new[] { "PrintBatch", "ClientId", "JobName" };

            var defaultedColumns = batchSummarySchema
                .Where(f => !usedColumns.Contains(f.FieldName) && !skippedColumns.Contains(f.FieldName)).ToList();

            defaultedColumns.ForEach(f =>
            {
                f.StrValue = f.DefaultValue;
            });

            var colParamPairs = usedColumns.Concat(defaultedColumns.Select(f => f.FieldName))
                .Select((c, i) => "[{0}] = @p{1}".Fill(c, i + 2)).ToArray();

            var defaultValues = defaultedColumns.Select(c => c.Value).ToList();

            var usedColumnValues = new object[] { batchSummary.PrintBatch, batchSummary.PrintFileName, batchSummary.JulianDate, batchSummary.Images, batchSummary.DueDate, batchSummary.SLADays, batchSummary.ValidationStatus, batchSummary.OnValidationErrors, batchSummary.ValidationErrors, batchSummary.BatchIdentifier, batchSummary.PrintTrailId, batchSummary.PrintLocation, batchSummary.DocTypeId };

            var allValues = usedColumnValues.Concat(defaultValues).ToArray();

            var colParamList = String.Join(", ", colParamPairs);

            var sql = "UPDATE [PnS].[BatchSummary] SET {0} WHERE PrintBatch = @p1".Fill(colParamList);

            db.ExecuteSqlNonQuery(sql, allValues.ToArray());
        }

        public static List<PCIField> GetBatchSummarySchema()
        {
            var ds = db.ExecuteDataSet("usp_GetBatchSummarySchema");
            return GetSchema(ds.Tables[0]);
        }

        public static List<PCIField> GetMRDFRequestSchema()
        {
            var ds = db.ExecuteDataSet("usp_GetMRDFRequestSchema");
            return GetSchema(ds.Tables[0]);
        }

        public static List<PCIField> GetMRDFResponseSchema()
        {
            var ds = db.ExecuteDataSet("usp_GetMRDFResponseSchema");
            return GetSchema(ds.Tables[0]);
        }

        private static List<PCIField> GetSchema(DataTable t)
        {
            var regex = new Regex(@"[^(]*");

            var isMRDFResponse = !t.Columns.Contains("DefaultValue");

            var schema = t.AsEnumerable()
                .Select(row => new PCIField
                {
                    Id = row.Field<int>("Id"),
                    FieldName = row.Field<string>("FieldName"),
                    StartPosition = row.Field<int>("StartPosition"),
                    Length = row.Field<int>("Length"),
                    IsActive = row.Field<bool>("IsActive"),
                    DataType = regex.Match(row.Field<string>("DataType")).Value.ParseEnum<SqlDbTypeCustom>(true),
                    DataFormatTemplate = row.Field<string>("DataFormatTemplate"),
                    DefaultValue = isMRDFResponse ? null : row.Field<string>("DefaultValue")
                });

            return schema.ToList();
        }

        public static void InsertMRDFRequestData(List<PCIField> mrdfRequestSchema, List<MRDFRequest> mrdfRequest)
        {
            var usedColumns = new[] { "JobId", "PieceId", "Date", "TotalSheets", "Address1", "UniqueId", "PDFFileName", "MailPieceStatus", "ErrId", "ErrDesc", "Expansion", "DivertVS3", "Feeder4_Sel" };
            var defaultedColumns = mrdfRequestSchema
                .Where(f => !usedColumns.Contains(f.FieldName)).ToList();

            defaultedColumns.ForEach(f =>
            {
                f.StrValue = f.DefaultValue;
            });

            var allColumns = usedColumns.Concat(defaultedColumns.Select(f => f.FieldName))
                .Select(c => "[{0}]".Fill(c));

            var colList = String.Join(", ", allColumns);


            var paramList = String.Join(", ", allColumns.Select((c, i) => "@p{0}".Fill(i + 1)));

            var defaultValues = defaultedColumns.Select(c => c.Value).ToList();

            foreach (var mrdf in mrdfRequest)
            {
                var usedColumnValues = new object[] { mrdf.JobId, mrdf.PieceId, mrdf.Date, mrdf.TotalSheets, mrdf.Address1, mrdf.UniqueId, mrdf.PDFFileName, mrdf.MailPieceStatus, mrdf.ErrId, mrdf.ErrDesc, mrdf.Expansion, mrdf.DivertVS3, mrdf.Feeder4_Sel };

                var values = usedColumnValues.Concat(defaultValues);

                var sql = "INSERT INTO [PnS].[MRDFRequest] ({0}) VALUES ({1})".Fill(colList, paramList);

                db.ExecuteSqlNonQuery(sql, values.ToArray());
            }
        }

        public static DataTable GetMRDFRequestDataByJobId(int jobId, string mailPieceStatus = "ALL")//To Get All Values define "ALL", Records without pdf level errors use "INTERNAL"
        {
            return db.ExecuteDataSet("usp_GetMRDFRequestDataByJobId", jobId, mailPieceStatus).Tables[0];
        }

        public static DataTable GetMRDFResponseDummyDataByJobId(int jobId)//To Get All Values define "ALL", Records without pdf level errors use "INTERNAL"
        {
            return db.ExecuteDataSet("usp_GetMRDFResponseDummyDataByJobId", jobId).Tables[0];
        }

        public static int DeleteMRDFRequestByJobId(int jobId)
        {
            return db.ExecuteNonQuery("usp_DeleteMRDFRequestByJobId", jobId);
        }

        public static void InsertMRDFResponseData(int jobId, List<PCIFieldData> mrdfResponseList)
        {
            if (mrdfResponseList.Count == 0)
                return;
            var allColumns = mrdfResponseList.First().PCIFieldColl.Where(x => x.FieldName != "Expansion").OrderBy(f => f.Id).Select(f => f.FieldName).ToList();
            var colList = String.Join(", ", allColumns);
            var paramList = String.Join(", ", allColumns.Select((c, i) => "@p{0}".Fill(i + 1)));

            foreach (var res in mrdfResponseList)
            {
                var sql = @"DELETE FROM [PnS].[MRDFResponse] Where JobId=@p1 and PieceId=@p2";
                db.ExecuteSqlNonQuery(sql, jobId, res.PieceId);

                var allValues = res.PCIFieldColl.Where(x => x.FieldName != "Expansion").OrderBy(f => f.Id).Select(f => f.Value).ToArray();
                sql = "INSERT INTO [PnS].[MRDFResponse] ({0}) VALUES ({1})".Fill(colList, paramList);

                db.ExecuteSqlNonQuery(sql, allValues);
            }
        }

        public static void UpdateMRDFRequestStatusFromResponse(EnumerableRowCollection<DataRow> mrdfResponseData, List<int> resPieceIds)
        {
            foreach (var pieceId in resPieceIds)
            {
                var res = mrdfResponseData.Single(row => row.Field<int>("PieceId") == pieceId);

                string errorUpdate = res.Field<string>("Disposition").Trim() == "7" ?
                    ", ErrId=7, ErrDesc='System was unable to print or ship this mailpiece. It has to be resent after necessary amendments.'" : "";

                var updateSql = "UPDATE [PnS].[MRDFRequest] SET MailPieceStatus=@p1{0} WHERE PrintTrailBatchId=@p2 AND PieceId=@p3".Fill(errorUpdate);

                db.ExecuteSqlNonQuery(updateSql, GetMailPieceStatus(res.Field<string>("Disposition").Trim()).ToString(),
                    res.Field<int>("JobId"),
                    res.Field<int>("PieceId"));
            }
        }

        public static DataTable GetMRDFResponseDataByJobId(int jobId)
        {
            var ds = db.ExecuteDataSet("usp_GetMRDFResponseDataByJobId", jobId);//Here we will create view instaed of SP
            return ds.Tables[0];
        }

        public static DataTable GetFinalMRDFResponseDataByJobId(int jobId)
        {
            var ds = db.ExecuteDataSet("usp_GetFinalMRDFResponseDataByJobId", jobId);//Here we will create view instaed of SP
            return ds.Tables[0];
        }

        public static List<MailPieceReport> GetMailPieceReportList(DataTable dt)
        {
            List<MailPieceReport> mailPieceReport = new List<MailPieceReport>();
            foreach (DataRow row in dt.Rows)
            {
                MailPieceReport mailPiece = new MailPieceReport();
                mailPiece.ErrDesc = row.Field<string>("ErrDesc");
                mailPiece.ErrId = row.Field<int>("ErrId");
                mailPiece.MailPieceStatus = row.Field<string>("MailPieceStatus").ParseEnum<MailPieceStatus>();
                mailPiece.PDFFileName = row.Field<string>("PDFFileName");
                mailPiece.PostageCost = row.Field<decimal>("PostageCost");
                mailPiece.PulledBackOn = mailPiece.MailPieceStatus == MailPieceStatus.PulledBack ? DateTime.SpecifyKind(row.Field<DateTime>("ShippedOn"), DateTimeKind.Utc) : DateTime.MinValue;
                mailPiece.ShippedOn = row.Field<DateTime?>("ShippedOn") == null || mailPiece.MailPieceStatus == MailPieceStatus.Error || mailPiece.MailPieceStatus == MailPieceStatus.PulledBack ? DateTime.MinValue : DateTime.SpecifyKind(row.Field<DateTime>("ShippedOn"), DateTimeKind.Utc);
                mailPiece.PrintedOn = mailPiece.ShippedOn;
                mailPiece.TotalSheets = row.Field<int?>("TotalSheets") == null ? 0 : row.Field<int>("TotalSheets");
                mailPiece.UniqueId = row.Field<Guid>("UniqueId");
                mailPiece.WeightMeasured = row.Field<decimal>("WeightMeasured");
                mailPiece.IsQCPerformed = row.Field<string>("DivertVS3").Trim() == "1" && row.Field<string>("Disposition").Trim() == "8";
                mailPiece.TrackingNumber = row.Field<DateTime?>("ShippedOn") == null || mailPiece.MailPieceStatus == MailPieceStatus.Error || mailPiece.MailPieceStatus == MailPieceStatus.PulledBack || string.IsNullOrEmpty(row.Field<string>("PackageTrackingNumber")) ? string.Empty : row.Field<string>("PackageTrackingNumber").Trim();
                mailPieceReport.Add(mailPiece);
            }
            return mailPieceReport;
        }

        private static MailPieceStatus GetMailPieceStatus(string dispositionId)
        {
            switch (dispositionId)
            {
                case "-1": return MailPieceStatus.Error;//This is used for files that were not sent for printing from our side due to some issue
                case "0": return MailPieceStatus.Shipped;
                case "1": return MailPieceStatus.Shipped;
                case "2": return MailPieceStatus.Shipped;
                case "3": return MailPieceStatus.Shipped;
                case "5": return MailPieceStatus.Shipped;
                case "6": return MailPieceStatus.Shipped;
                case "8": return MailPieceStatus.Shipped;//Reprinted on PCI side or QCDiverted Successfully and printed and shipped
                case "9": return MailPieceStatus.Shipped;
                case "7": return MailPieceStatus.Error;//This is used for files that were sent for printing but on PCI side had some issue while printing
                case "4": return MailPieceStatus.PulledBack;
                default:
                    return MailPieceStatus.Unknown;
            }
        }

        public static void InsertPageSizeWisePageCounts(Dictionary<string, int> pageSizewiseCounts, Guid mailPieceIdentifier)
        {
            string getQuery = @"SELECT Id FROM [AM].[MailPieces] WHERE UniqueId=@p1";
            var mailPieceId = db.ExecuteSqlScalar<int?>(getQuery, mailPieceIdentifier);
            if (mailPieceId.HasValue)
            {
                foreach (var pageCount in pageSizewiseCounts)
                {
                    //try to delete if any record then try to insert
                    var sqlDelete = @"DELETE FROM [Common].[MailPiecePageInfo] Where MailPieceId=@p1 AND PageSize=@p2";
                    db.ExecuteSqlNonQuery(sqlDelete, mailPieceId.Value, pageCount.Key);

                    var sqlInsert = @"INSERT INTO [Common].[MailPiecePageInfo]([MailPieceId],[PageSize],[PageCount]) VALUES(@p1,@p2,@p3)";
                    db.ExecuteSqlNonQuery(sqlInsert, mailPieceId.Value, pageCount.Key, pageCount.Value);
                }
            }
        }

        public static DataTable GetPullBackRequestDataByJobId(int jobId)
        {
            var ds = db.ExecuteDataSet("usp_GetPullBackRequestDataByJobId", jobId);
            return ds.Tables[0];
        }

        public static DataTable GetMRDFResponseDataByPrintTrailBatchId(int printTrailBatchId)
        {
            var ds = db.ExecuteDataSet("usp_GetMRDFResponseDataByPrintTrailBatchId", printTrailBatchId);
            return ds.Tables[0];
        }

        public static DataTable GetMRDFResponseDummyDataByPrintTrailBatchId(int printTrailBatchId)//To Get All Values define "ALL", Records without pdf level errors use "INTERNAL"
        {
            return db.ExecuteDataSet("usp_GetMRDFResponseDummyDataByPrintTrailBatchId", printTrailBatchId).Tables[0];
        }

        public static DataTable GetMRDFRequestDataByPrintTrailBatchId(int printTrailBatchId, string mailPieceStatus = "ALL")//To Get All Values define "ALL", Records without pdf level errors use "INTERNAL"
        {
            var ds = db.ExecuteDataSet("usp_GetMRDFRequestDataByPrintTrailBatchId", printTrailBatchId, mailPieceStatus);//Here we will create view instaed of SP
            return ds.Tables[0];
        }
    }
}
