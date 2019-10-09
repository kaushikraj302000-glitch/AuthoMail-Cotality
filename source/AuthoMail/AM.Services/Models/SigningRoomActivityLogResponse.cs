using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class SigningRoomActivityLogResponse
    {
        public int SigningRoomId { get; set; }
        public string SigningRoomActivityType { get; set; }
        public int? ParticipantId { get; set; }
        public string ParticipantFirstName { get; set; }
        public string ParticipantLastName { get; set; }
        public string ParticipantEmail { get; set; }
        public string IPAddress { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Error { get; set; }
        public bool HasError
        {
            get
            {
                return !string.IsNullOrEmpty(this.Error) ? true : false;
            }
        }
    }
}
