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
                //DirectoryInfo sourceAMRootDir = new DirectoryInfo(@"E:\Delete\AMRoot");
                //string sourcePnSRootDir = @"E:\Delete\PCIRoot";
                //string destAMDir = @"E:\Delete\AMTrash";
                //string destPCIDir = @"E:\Delete\PCITrash";

                //Prod Paths
                DirectoryInfo sourceAMRootDir = new DirectoryInfo(@"D:\Production\AuthoMail\AMRoot");
                string destAMDir = @"D:\MTC\Delete\AM";
                DirectoryInfo sourcePnSRootDir = new DirectoryInfo(@"D:\MTC\PnSService\PCIRoot");
                string destPCIDir = @"D:\MTC\Delete\PCI";

                //Staging Paths
                //DirectoryInfo sourceAMRootDir = new DirectoryInfo(@"\\10.215.112.64\d$\MTC\AuthoMail\Monitor\AuthoMailRoot");
                //string destAMDir = @"\\10.215.112.64\d$\MTC\Delete\AM";
                //DirectoryInfo sourcePnSRootDir = new DirectoryInfo(@"\\10.215.112.64\d$\MTC\Deployment\PCIRoot");
                //string destPCIDir = @"\\10.215.112.64\d$\MTC\Delete\PCI";

                var amDirs = sourceAMRootDir.EnumerateDirectories("*.*", SearchOption.TopDirectoryOnly);
                foreach (var dir in amDirs)
                {
                    if (!Directory.Exists(Path.Combine(destAMDir, dir.Name)))
                        Directory.CreateDirectory(Path.Combine(destAMDir, dir.Name));

                    var subdirs = dir.EnumerateDirectories().Where(d => d.LastWriteTime < Common.Today.AddMonths(-1)).ToList();

                    foreach (var subdir in subdirs)
                    {
                        if (Directory.Exists(Path.Combine(Path.Combine(destAMDir, dir.Name), subdir.Name)))
                            Directory.Delete(Path.Combine(Path.Combine(destAMDir, dir.Name), subdir.Name));
                        Directory.Move(subdir.FullName, Path.Combine(Path.Combine(destAMDir, dir.Name), subdir.Name));
                    }
                }

                var pnsDirs = sourcePnSRootDir.EnumerateDirectories("*.*", SearchOption.TopDirectoryOnly);
                foreach (var dir in pnsDirs)
                {
                    if (!Directory.Exists(Path.Combine(destPCIDir, dir.Name)))
                        Directory.CreateDirectory(Path.Combine(destPCIDir, dir.Name));

                    var subdirs = dir.EnumerateDirectories().Where(d => d.LastWriteTime < Common.Today.AddMonths(-2)).ToList();

                    foreach (var subdir in subdirs)
                    {
                        if (Directory.Exists(Path.Combine(Path.Combine(destPCIDir, dir.Name), subdir.Name)))
                            Directory.Delete(Path.Combine(Path.Combine(destPCIDir, dir.Name), subdir.Name));
                        Directory.Move(subdir.FullName, Path.Combine(Path.Combine(destPCIDir, dir.Name), subdir.Name));
                    }
                }
            }
        }

    }
}
