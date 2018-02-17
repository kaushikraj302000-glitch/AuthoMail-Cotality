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
        public bool IsClientActive { get; set; }
        public bool IsPnSActive { get; set; }
        public string SFTProtocol { get; set; }
        public string SFTHost { get; set; }
        public string SFTUserName { get; set; }
        public string SFTPassword { get; set; }
        public string SFTADERequestDir { get; set; }
        public string SFTADERequestArchiveDir { get; set; }
        public string SFTADEResponseDir { get; set; }
        public string TimeZone { get; set; }
    }
}
