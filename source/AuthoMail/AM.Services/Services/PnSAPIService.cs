using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services.Services
{
    public interface IPnSAPIService : IDisposable
    {
        Task<AddPackageResult> AddPackage(Guid? id, DateTime receivedOn);
        Task UpdatePackageStatus(RequestLog requestLog);
    }

    public class PnSAPIService : ServiceBase, IPnSAPIService
    {
        private const string Verifying = "Verifying";
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
                var responseHttpStatusCode = 0;
                bool isPrimary = false;

                if (!id.HasValue || id == Guid.Empty)
                {
                    isPrimary = true;
                    responseHttpStatusCode = 0;
                }
                else
                {
                    // check if request is successfully accepted before
                    var primaryHTTPStatusCode = await db.ExecuteScalarAsync<int?>(@"
                        select HTTPStatusCode from API.AMRequestLogs
                        where MailPieceId = @id and IsPrimary = 1
                    ", new { id }, transaction: t);

                    // 406 and 409 get higher priority than 400
                    // this ensures that even if a request is tried 100 times (more than max limit)
                    // they still get 406 or 409 if applicable

                    if (primaryHTTPStatusCode == 0)
                    {
                        // Earlier request is still in progress
                        isPrimary = false;
                        responseHttpStatusCode = 406;
                    }
                    else if (primaryHTTPStatusCode == 200)
                    {
                        // request has already been accepted
                        isPrimary = false;
                        responseHttpStatusCode = 409;
                    }
                    else
                    {
                        /// request is received for the first time
                        /// or multiple times less than max retries
                        /// or more than max retries

                        // it is not 0 or 200 so it becomes primary now
                        isPrimary = true;

                        // check if the request is received multiple times
                        var reqCount = await db.ExecuteScalarAsync<int>(@"
                            select count(Id) from API.AMRequestLogs where MailPieceId = @id
                        ", new { id }, transaction: t);

                        responseHttpStatusCode = reqCount >= Config.MailRequestAPI_MaxRetries ? 400 : 0;
                    }

                }

                // record current request
                var reqId = await db.ExecuteScalarAsync<int>(@"
                    insert into API.AMRequestLogs (MailPieceId, IsPrimary, Status, HTTPStatusCode, ReceivedOn)
                    OUTPUT INSERTED.ID
                    values (@id, @isPrimary, @status, @httpStatusCode, @receivedOn)
                ", new { id, isPrimary, status = Verifying, httpStatusCode = 0, receivedOn }, transaction: t);

                t.Commit();
                db.Close();

                return new AddPackageResult { Id = reqId, HTTPStatusCode = responseHttpStatusCode };
            }
        }

        public async Task UpdatePackageStatus(RequestLog requestLog)
        {
            await db.OpenAsync();
            // update package status
            using (var t = db.BeginTransaction())
            {
                requestLog.Status = requestLog.Status == "OK" ? "Queued" : requestLog.Status;

                await db.ExecuteAsync(@"
                    update API.AMRequestLogs
                    set Status = @Status,
                        HTTPStatusCode = @HTTPStatusCode,
                        APIResponse = @APIResponse,
                        ReceivedOn = @ReceivedOn,
                        ProcessedOn = @ProcessedOn,
                        ErrorLog = @ErrorLog,
                        InitialChecks = @InitialChecks,
                        Download = @Download,
                        Validate = @Validate,
                        [Merge] = @Merge,
                        PostBatch = @PostBatch,
                        Cleanup = @Cleanup,
                        TotalTime = @TotalTime
                    where Id = @Id
                ", requestLog, transaction: t);

                t.Commit();
                db.Close();
            }
        }
    }
}
