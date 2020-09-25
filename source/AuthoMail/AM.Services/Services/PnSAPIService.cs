using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services.Services
{
    public interface IPnSAPIService : IDisposable
    {
        Task<AddPackageResult> AddPackage(Guid? id, DateTime receivedOn);
        Task UpdatePackageStatus(RequestLog perfLog);
    }

    public class PnSAPIService : ServiceBase, IPnSAPIService
    {
        private const string Processing = "Processing";
        //public PnSAPIService() : base("AMAPI") { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="receivedOn"></param>
        /// <returns>Returns HTTP reponse status codes 200, 400, 406, 409</returns>
        public async Task<AddPackageResult> AddPackage(Guid? id, DateTime receivedOn)
        {
            await db.OpenAsync();

            using (var t = db.BeginTransaction())
            {
                // check if the request is received multiple times
                var reqCount = await db.ExecuteScalarAsync<int>(@"
                    select count(Id) from API.AMRequestLogs where MailPieceId = @id
                    ",new { id }, transaction: t);

                // check if request is successfully accepted before

                var primaryHTTPStatusCode = await db.ExecuteScalarAsync<int>(@"
                    select HTTPStatusCode from API.AMRequestLogs
                    where MailPieceId = @id and IsPrimary = 1
                    ", new { id }, transaction: t);

                var isMailPieceAccepted = primaryHTTPStatusCode == 200;

                var status = Processing;
                var isPrimary = !isMailPieceAccepted;

                // record current request
                var reqId = await db.ExecuteScalarAsync<int>(@"
                    insert into API.AMRequestLogs (MailPieceId, IsPrimary, Status, ReceivedOn)
                    OUTPUT INSERTED.ID
                    values (@id, @isPrimary, @status, @receivedOn)",
                    new { id, isPrimary, status, receivedOn }, transaction: t);



                if (reqCount >= Config.MailRequestAPI_MaxRetries)
                {
                    t.Commit();
                    return new AddPackageResult { Id = reqId, HTTPStatusCode = 400 };
                }

                var packageExists = await db.ExecuteScalarAsync<bool>("" +
                    "select 1 from Packages where Id = @id");

                if (packageExists)
                {
                    // if yes, return false and log request
                    var processedOn = Common.Now;
                    var totalTime = processedOn.Subtract(receivedOn).TotalSeconds;

                    await db.ExecuteAsync("" +
                        "insert into DuplicateRequets (UniqueId, ReceivedOn, ProcessedOn, TotalTime) " +
                        "values (@id, @receivedOn, @processedOn, @totalTime",
                        new { id, receivedOn, processedOn, totalTime }, transaction: t);
                }
                else
                {
                    // if no, add package and return true
                    //var status = ReqStatus.Verifying;

                    await db.ExecuteAsync("" +
                        "insert into Packages (Id, Status, ReceivedOn, ProcessedOn, ErrorLog) " +
                        "values (@id, @status, @receivedOn, NULL, NULL",
                        new { id, status, receivedOn }, transaction: t);
                }

                t.Commit();
                db.Close();
                throw new NotImplementedException();
            }
        }

        public async Task UpdatePackageStatus(RequestLog perfLog)
        {
            await db.OpenAsync();
            // update package status
            using (var t = db.BeginTransaction())
            {

                await db.ExecuteAsync("" +
                    "update Packages set" +
                    "   Status = @Status, " +
                    "   ProcessedOn = @ProcessedOn, " +
                    "   ErrorLog = @ErrorLog " +
                    "where id = @UniqueId", perfLog, transaction: t);

                await db.ExecuteAsync("" +
                    "insert into PerfLog (Id, SizeCheck, Download, Validate, Merge, PostBatch, Cleanup, TotalTime) " +
                    "values (@UniqueId, @SizeCheck, @Download, @Validate, @Merge, @PostBatch, @Cleanup, @TotalTime)"
                    , perfLog, transaction: t);

                t.Commit();
                db.Close();
            }
        }
    }
}
