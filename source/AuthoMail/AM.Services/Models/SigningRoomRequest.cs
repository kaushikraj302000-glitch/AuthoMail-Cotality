using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class SigningRoomRequest
    {
        public string SigningRoomName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public List<Participant> Participants { get; set; }
        [JsonProperty("Documents")]
        public List<DocumentInfo> Documents { get; set; }
    }

    public class Participant
    {
        public string FirstName { get; set; }
        public string LastName{ get; set; }
        public string InvitationEmail { get; set; }
    }

    public class DocumentInfo
    {
        public string DocumentName { get; set; }
        public string FileName { get; set; }
        public string DocumentData { get; set; }
    }
}
