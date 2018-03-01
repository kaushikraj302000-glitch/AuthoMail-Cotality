using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities.IdentityModels
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ProfileUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ProfileUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            //userIdentity.AddClaim(new Claim("clientId", ClientId == null ? string.Empty : ClientId.ToString()));
            userIdentity.AddClaim(new Claim("ClientId", this.ClientId.ToString()));
            return userIdentity;
        }
        
    }
    public static class IdentityExtensions
    {
        public static string GetClientId(this IIdentity identity)
        {
           
            var claim = ((ClaimsIdentity)identity).FindFirst("clientId");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
    }

}
