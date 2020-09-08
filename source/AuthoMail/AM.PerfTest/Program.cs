using AM.Entities;
using AM.Lib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AM.PerfTest
{
    public class Program
    {
        static async Task Main(string[] args)
        {

            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    if (args.Length > 0)
                    {
                        config.AddJsonFile($"appsettings.{args[0]}.json", true, true);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    #region snippet3
                    var config = hostContext.Configuration;

                    services.AddSingleton<App>();
                    services.Configure<Config>(config.GetSection(nameof(Config)));
                    #endregion
                })
                .Build();

            await host.StartAsync();

            var app = host.Services.GetRequiredService<App>();

            await app.RunAsync(args);

            await host.WaitForShutdownAsync();
            
        }

       

        private static void ExtractBatchAndSendRequest()
        {
            string batchPath = @"Batches";
            string extractLocation = @"ExtractedBatches";
            foreach (var zipFilePath in Directory.GetFiles(batchPath, "*.zip"))
            {
                string extractBatchPath = Path.Combine(extractLocation, Path.GetFileNameWithoutExtension(zipFilePath));
                ZipFile.ExtractToDirectory(zipFilePath, extractBatchPath, true);

                string xmlString = File.ReadAllText(Directory.GetFiles(extractBatchPath, "*.xml").First());
                try
                {
                    var mailPiece = XmlUtil.Deserialize<MailPiece>(xmlString);
                    mailPiece.Id = Guid.NewGuid();
                    //var response = await SendMailRequest(mailPiece);
                }
                catch (Exception)
                {

                    throw;
                }


            }
        }

    }
}
