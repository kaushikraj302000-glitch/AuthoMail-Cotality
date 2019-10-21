using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class SigningRoomRequest
    {
        [Description("This is a mandatory field which is used to identify signing room and must be unique.")]
        public string SigningRoomName { get; set; }
        [Description("This is an optional field which used to provide description of SigningRoom.")]
        public string Description { get; set; }
        [Description("This is used to set the start date and time of signing room.")]
        public DateTime StartDate { get; set; }
        [Description("This is used to set the expiration date and time of signing room.")]
        public DateTime ExpirationDate { get; set; }
        public List<Participant> Participants { get; set; }
        [JsonProperty("Documents")]
        public List<DocumentInfo> Documents { get; set; }
    }

    public class Participant
    {
        [Description("This is mandatory field to set the first name of participant.")]
        public string FirstName { get; set; }
        [Description("This is mandatory field to set the last name of participant.")]
        public string LastName{ get; set; }
        [Description("This is mandatory field to set the email address of participant.")]
        public string InvitationEmail { get; set; }
    }

    public class DocumentInfo
    {
        [Description("This is optional field to set the document name to upload.")]
        public string DocumentName { get; set; }
        [Description("This is optional field to set the file name to upload.")]
        public string FileName { get; set; }
        [Description("This is mandatory field to set the document data in base64 string.")]
        public string DocumentData { get; set; }
    }
}
