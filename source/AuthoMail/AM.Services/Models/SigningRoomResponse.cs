using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class SigningRoomResponse
    {
        public SigningRoom SigningRoom { get; set; }
        public List<string> Warnings { get; set; }
        public string Error { get; set; }
        public bool HasError
        {
            get
            {
                return !string.IsNullOrEmpty(this.Error) ? true : false;
            }
        }
        public bool HasWarnings
        {
            get
            {
                return Warnings.Count() > 0 ? true : false;
            }
        }
    }

    public class SigningRoom
    {
        public int SigningRoomId { get; set; }
        public Guid SigningRoomGuid { get; set; }
        public DateTime StartDate { get; set; }
        public bool Expired { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Status { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
