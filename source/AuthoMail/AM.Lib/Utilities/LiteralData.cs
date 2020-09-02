using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Lib
{
    public class LiteralData
    {
        public string FileName { get; set; }
        public int Format { get; set; }
        public Stream Stream { get; set; }

        internal LiteralData(PgpLiteralData pgpLiterlData)
        {
            FileName = pgpLiterlData.FileName;
            Format = pgpLiterlData.Format;
            Stream = pgpLiterlData.GetInputStream();
        }

        public async Task SaveAsFile(string filePath = null)
        {
            var destFile = filePath ?? FileName;

            using (var os = File.Open(destFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                await Stream.CopyToAsync(os);
            }
        }

        public string GetAsString()
        {
            using (var reader = new StreamReader(Stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
