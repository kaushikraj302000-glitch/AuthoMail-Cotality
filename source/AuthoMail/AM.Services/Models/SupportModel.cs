using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class ClientDomains
    {
        public int ClientId { get; set; }
        public string AllowedDomain { get; set; }
        public string ClientName { get; set; }
    }
    public class UserRequestSignUp
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public DateTime RequestedOn { get; set; }
        public string Status { get; set; }
    }
    public class SFTChannel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Protocol { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ToADEDir { get; set; }
        public string FromADEDir { get; set; }
        public string ArchiveDir { get; set; }
        public string PostDownloadAction { get; set; }
        public bool IsActive { get; set; }
    }
}
