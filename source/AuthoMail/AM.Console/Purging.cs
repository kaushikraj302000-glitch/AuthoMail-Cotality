using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    public class Purging
    {
        public static void StartPurging()
        {
            using (IReCalculateDueDateService rSvc = new ReCalculateDueDateService())
            {
                //Testing Paths
                //string sourceAMRootDir = @"E:\Delete\AMRoot";
                //string sourcePnSRootDir = @"E:\Delete\PCIRoot";
                //string destAMDir = @"E:\Delete\AMTrash";
                //string destPCIDir = @"E:\Delete\PCITrash";

                //Prod Paths
                //string sourceAMRootDir = @"\\10.212.24.57\d$\Production\AuthoMail\AMRoot";
                //string sourcePnSRootDir = @"\\10.212.24.57\d$\MTC\PnSService\PCIRoot";
                //string destAMDir = @"\\10.212.24.57\d$\MTC\Delete\AM";
                //string destPCIDir = @"\\10.212.24.57\d$\MTC\Delete\PCI";

                //Staging Paths
                string sourceAMRootDir = @"\\10.215.112.64\d$\MTC\AuthoMail\Monitor\AuthoMailRoot";
                string sourcePnSRootDir = @"\\10.215.112.64\d$\MTC\Deployment\PCIRoot";
                string destAMDir = @"\\10.215.112.64\d$\MTC\Delete\AM";
                string destPCIDir = @"\\10.215.112.64\d$\MTC\Delete\PCI";


                string sourceDirPath = string.Empty;
                var mailBatchesListToPurge = rSvc.GetMailBatchesListToPurge();

                foreach (var batch in mailBatchesListToPurge)
                {
                    sourceDirPath = Path.Combine(Path.Combine(sourceAMRootDir, batch.AppCode), batch.BatchId.ToString());
                    if (Directory.Exists(sourceDirPath))
                    {
                        DirectoryCopy(sourceDirPath, Path.Combine(Path.Combine(destAMDir, batch.AppCode), batch.BatchId.ToString()), true);
                        Directory.Delete(Path.Combine(Path.Combine(sourceAMRootDir, batch.AppCode), batch.BatchId.ToString()), true); 
                    }
                }


                var batchSummaryListToPurge = rSvc.GetBatchSummaryListToPurge();
                foreach (var batch in batchSummaryListToPurge)
                {
                    sourceDirPath = Path.Combine(Path.Combine(sourcePnSRootDir, batch.AppCode), batch.SearchPattern);
                    if (Directory.Exists(sourceDirPath))
                    {
                        DirectoryCopy(sourceDirPath, Path.Combine(Path.Combine(destPCIDir, batch.AppCode), batch.SearchPattern), true);
                        Directory.Delete(Path.Combine(Path.Combine(sourcePnSRootDir, batch.AppCode), batch.SearchPattern), true); 
                    }
                }
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
