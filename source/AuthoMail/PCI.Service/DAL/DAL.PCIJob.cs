using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace PCI.Service
{
    partial class DAL
    {
        public static List<SubmittedBatches> GetSubmittedBatches()
        {
            var submittedBatches = db.ExecuteDataSet("usp_GetSubmittedBatches").Tables[0];
            List<SubmittedBatches> printBatches = submittedBatches.AsEnumerable().Select(x => new SubmittedBatches
            {
                PrintBatchId = x.Field<int>("PrintBatch"),
                PatternBatchCode = "{0}-{1}-{2}-{3}-".Fill(x.Field<string>("CustCode"), x.Field<string>("BatchCode"), x.Field<string>("AppCode"), x.Field<string>("DocTypeCode")),
                ReceivedFileList = new List<string>()
            }).ToList();
            return printBatches;
        }

        public static List<SubmittedBatches> GetMRDFReceivedBatches()
        {
            var submittedBatches = db.ExecuteDataSet("usp_GetMRDFReceivedBatches").Tables[0];
            List<SubmittedBatches> printBatches = submittedBatches.AsEnumerable().Select(x => new SubmittedBatches
            {
                PrintBatchId = x.Field<int>("PrintBatch"),
                PatternBatchCode = "{0}-{1}-{2}-{3}-".Fill(x.Field<string>("CustCode"), x.Field<string>("BatchCode"), x.Field<string>("AppCode"), x.Field<string>("DocTypeCode")),
                ReceivedFileList = new List<string>()
            }).ToList();
            return printBatches;
        }

        public static List<MailPieceReceivedOnData> GetMailBatchReceivedOnDataBasedFromPrintBatchId(int pnsPrintBatchId)
        {
            var amMailPiecesData = db.ExecuteDataSet("usp_GetMailBatchReceivedOnDataBasedFromPrintBatchId", pnsPrintBatchId).Tables[0];
            List<MailPieceReceivedOnData> amMailPieces = amMailPiecesData.AsEnumerable().Select(x => new MailPieceReceivedOnData
            {
                MailPieceId = x.Field<int>("MailPieceId"),
                EffectiveReceivedOnEst = x.Field<DateTime>("EffectiveReceivedOnEst"),
                UniqueId = x.Field<Guid>("UniqueId"),
                MailPieceFileName = x.Field<string>("MailPieceFileName")
            }).ToList();

            return amMailPieces;
        }
    }
}
