using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AM.Services
{
    public class AccountUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public bool LockoutEnabled { get; set; }
        public int? ClientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public DateTime RequestedOn { get; set; }
        public string Status { get; set; }
        public DateTime? ClosedOn { get; set; }
        public string ClosedBy { get; set; }
        public List<string> Roles { get; set; }
        public virtual bool IsAPIUser { get; set; }

    }

}
