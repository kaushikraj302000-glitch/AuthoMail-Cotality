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
                    "select 1 from Packages where Id = @id", new { id });

                if (packageExists)
                {
                    // if yes, return false and log request
                    var processedOn = Common.Now;
                    var totalTime = processedOn.Subtract(receivedOn).TotalSeconds;

                    await db.ExecuteAsync("" +
                        "insert into DuplicateRequets (UniqueId, ReceivedOn, ProcessedOn, TotalTime) " +
                        "values (@id, @receivedOn, @processedOn, @totalTime",
                        new { id, receivedOn, processedOn, totalTime });
                }
                else
                {
                    // if no, add package and return true
                    var status = ReqStatus.Verifying;

                    await db.ExecuteAsync("" +
                        "insert into Packages (Id, Status, ReceivedOn, ProcessedOn, ErrorLog) " +
                        "values (@id, @status, @receivedOn, NULL, NULL",
                        new { id, status, receivedOn });
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



                t.Commit();
                db.Close();
            }
        }
    }
}
