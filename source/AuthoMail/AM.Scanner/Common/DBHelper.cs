using AM.Scanner.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Web;
using Dapper;
using System.Globalization;
using AM.Lib;
namespace AM.Scanner.Common
{
    public class DBHelper
    {
        internal static DbConnection Create(string connectionStringName)
        {
            var cs = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (cs == null)
            {
                throw new ArgumentException("Connection string with name '{0}' does not exist.", connectionStringName);
            }

            var factory = DbProviderFactories.GetFactory(cs.ProviderName);
            var connection = factory.CreateConnection();
            connection.ConnectionString = cs.ConnectionString;
            return connection;
        }

        public static ScanDetails RegisterScan(string barcode, string clientCode, string username, string machineName, string batchName)
        {
            barcode = barcode.Replace(clientCode, "");
            try
            {
                DbConnection db = Create(clientCode);
                //DbCommand cmd = db.GetStoredProcCommand(Common.SP_RegisterScan, barcode);
                // DbCommand cmd = db.GetStoredProcCommand(Common.SP_RegisterScan, barcode, username, machineName);
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                string query = string.Format("{0} @barcode, @clientCode, @operatorid, @machineid, @batchName, @estDate", Common.SP_RegisterScan);
                var registerScanData = db.QuerySingleOrDefault<RegisterScanDetails>(query, new { barcode = barcode, clientCode = clientCode, operatorid = username, machineid = machineName, batchName = batchName, estDate = estToday});
                db.Dispose();
                var scanCountData = GetScanStats(clientCode);
                scanCountData.JobName = registerScanData.JobName;
                scanCountData.Code = registerScanData.Code;
                scanCountData.Message = registerScanData.Message;
                scanCountData.AppCode = clientCode;
                scanCountData.TrackingNo = registerScanData.TrackingNo;
                return scanCountData;

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static ScanDetails GetScanStats(string clientCode)
        {
            try
            {
                DbConnection db = Create(clientCode);
                string scanCountQuery = string.Format("{0} @appcode, @estDate, @startdate, @enddate", Common.SP_GetScanStats);
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                // EST Week
                var startDate = (estToday + new TimeSpan(0, 0, 0)).EstToUtc();
                var endDate = (estToday + new TimeSpan(23, 59, 59)).EstToUtc();
                var scanCountData = db.QuerySingleOrDefault<ScanDetails>(scanCountQuery, new { appcode = clientCode, estDate = estToday, startDate = startDate, endDate = endDate });
                db.Dispose();
                return scanCountData;

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static List<RegisterScanDetails> GetScannedMailPieces(string clientCode,string batchName)
        {
            try
            {
                DbConnection db = Create(clientCode);
                string scanCountQuery = string.Format("{0} @appcode, @batchName, @startdate, @enddate, @estDate", Common.SP_GetScannedMailPieces);
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                // EST Week
                var startDate = (estToday + new TimeSpan(0, 0, 0)).EstToUtc();
                var endDate = (estToday + new TimeSpan(23, 59, 59)).EstToUtc();
                var scannedMailPieceList = db.Query<RegisterScanDetails>(scanCountQuery, new { appcode = clientCode, batchName = batchName, startDate = startDate, endDate = endDate, estDate=estToday}).ToList();
                db.Dispose();
                return scannedMailPieceList;

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static List<GetAllBatchCountDetails> GetRecentBatchesStats(string clientCode)
        {
            try
            {
                DbConnection db = Create(clientCode);
                string scanCountQuery = string.Format("{0} @appcode, @estDate, @startdate, @enddate", Common.SP_GetRecentBatchesStats);
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                // EST Week
                var startDate = (estToday + new TimeSpan(0, 0, 0)).EstToUtc();
                var endDate = (estToday + new TimeSpan(23, 59, 59)).EstToUtc();
                var batchList = db.Query<GetAllBatchCountDetails>(scanCountQuery, new { appcode = clientCode, estDate=estToday, startDate = startDate, endDate = endDate }).ToList();
                db.Dispose();
                return batchList;

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static List<ScanedDocument> GetScanReportData(string clientCode, string barcodeValue, string loanNumber, string printOnFrom, string printOnTo, string shippedOnFrom, string shippedOnTo)
        {
            try
            {
                //barcodeValue = string.IsNullOrEmpty(barcodeValue.Trim()) ? null : barcodeValue.Trim();
                //loanNumber = string.IsNullOrEmpty(loanNumber.Trim()) ? null : loanNumber.Trim();
                //printOnFrom = string.IsNullOrEmpty(printOnFrom.Trim()) ? null : printOnFrom.Trim();
                //printOnTo = string.IsNullOrEmpty(printOnTo.Trim()) ? null : printOnTo.Trim();
                //shippedOnFrom = string.IsNullOrEmpty(shippedOnFrom.Trim()) ? null : shippedOnFrom.Trim();
                //shippedOnTo = string.IsNullOrEmpty(shippedOnTo.Trim()) ? null : shippedOnTo.Trim();

                DateTime printOnStart = (DateTime.ParseExact(printOnFrom.Trim(), "MMMM d,yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).Date + new TimeSpan(0, 0, 0)).EstToUtc();

                DateTime printOnEnd = (DateTime.ParseExact(printOnTo.Trim(), "MMMM d,yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).Date + new TimeSpan(23, 59, 59)).EstToUtc();

                DateTime shippedOnStart = (DateTime.ParseExact(shippedOnFrom.Trim(), "MMMM d,yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).Date + new TimeSpan(0, 0, 0)).EstToUtc();

                DateTime shippedOnEnd = (DateTime.ParseExact(shippedOnTo.Trim(), "MMMM d,yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).Date + new TimeSpan(23, 59, 59)).EstToUtc();

                DbConnection db = Create(clientCode);
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                string query = string.Format("{0} @loanNumber, @trackingNo, @appcode, @estDate, @substartdate, @subenddate, @scanstartDate,@scanendDate", Common.SP_GetScanReportData);

                var batchList = db.Query<ScanedDocument>(query, new { loanNumber = loanNumber, trackingNo = barcodeValue, appcode = clientCode, estDate = estToday, substartdate = shippedOnStart, subenddate = shippedOnEnd, scanstartDate = printOnStart, scanendDate = printOnEnd }).ToList();
                db.Dispose();
                return batchList;

            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<PendingScanning> GetPendingScanning(string clientCode)
        {
            try
            {
                DbConnection db = Create(clientCode);
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                string query = string.Format("{0} @appcode, @estDate", Common.SP_GetPendingScanning);
                var batchList = db.Query<PendingScanning>(query, new { appcode = clientCode, estDate = estToday }).ToList();
                db.Dispose();
                return batchList;
                
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static USPSScanDetails RegisterUSPSScan(int mailPieceId,string trackingNo, string clientCode)
        {
            
            try
            {
                DbConnection db = Create(clientCode);
                //DbCommand cmd = db.GetStoredProcCommand(Common.SP_RegisterScan, barcode);
                // DbCommand cmd = db.GetStoredProcCommand(Common.SP_RegisterScan, barcode, username, machineName);
                string query = string.Format("{0} @mailPieceId,@trackingNo", Common.SP_RegisterUSPSScan);
                var registerScanData = db.QuerySingleOrDefault<USPSScanDetails>(query, new { mailPieceId = mailPieceId, trackingNo = trackingNo });
                registerScanData.AppCode = clientCode;
                db.Dispose();
                return registerScanData;

            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}