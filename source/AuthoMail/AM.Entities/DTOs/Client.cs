using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class Client
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string AppCode { get; set; }
        public int BusinessUnit { get; set; }
        public bool IsAMActive { get; set; }
        public bool IsPnSActive { get; set; }
        public string TimeZone { get; set; }
    }

    public class ClientSFTChannel
    {
        public int ClientId { get; set; }
        public int SFTChannelId { get; set; }
    }

    public class SFTChannel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Protocol { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ADERequestDir { get; set; }
        public string ADERequestArchiveDir { get; set; }
        public string ADEResponseDir { get; set; }
        public string PostDownloadAction { get; set; }
    }
}
