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
        Task<bool> TryAddPackage(string id, DateTime receivedOn);
        Task UpdatePackageStatus(PerfLog perfLog);
    }

    public class PnSAPIService : ServiceBase, IPnSAPIService
    {
        public PnSAPIService() : base("AMAPI") { }

        public async Task<bool> TryAddPackage(string id, DateTime receivedOn)
        {
            await db.OpenAsync();

            using (var t = db.BeginTransaction())
            {
                // check if the package already received

                var packageExists = await db.ExecuteScalarAsync<bool>("" +
                    "select 1 from Packages where Id = @id", new { id }, transaction: t);

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
                    var status = ReqStatus.Verifying;

                    await db.ExecuteAsync("" +
                        "insert into Packages (Id, Status, ReceivedOn, ProcessedOn, ErrorLog) " +
                        "values (@id, @status, @receivedOn, NULL, NULL",
                        new { id, status, receivedOn }, transaction: t);
                }

                t.Commit();
                db.Close();
                return !packageExists;
            }
        }

        public async Task UpdatePackageStatus(PerfLog perfLog)
        {
            // update package status
            using (var t = db.BeginTransaction())
            {
                await db.OpenAsync();

                await db.ExecuteAsync("" +
                    "update Packages set" +
                    "   Status = @Status, " +
                    "   ProcessedOn = @ProcessedOn, " +
                    "   ErrorLog = @ErrorLog " +
                    "where id = @UniqueId", perfLog, transaction: t);

                await db.ExecuteAsync("" +
                    "insert into PerfLog (Id, SizeCheck, Download, Validate, Merge, PostBatch, Cleanup, TotalTime) " +
                    "value (@Id, @SizeCheck, @Download, @Validate, @Merge, @PostBatch, @Cleanup, @TotalTime)"
                    , perfLog, transaction: t);

                t.Commit();
                db.Close();
            }
        }
    }
}
