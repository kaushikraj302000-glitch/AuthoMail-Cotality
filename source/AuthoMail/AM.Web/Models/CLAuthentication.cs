using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class CLAuthentication
    {
        public static bool HasCorelogicAccount(string Email)
        {
            if (Email == null)
            {
                return false;
            }
            if (Email.IndexOf('@') == -1)
            {
                return false;
            }
            if (Email.Split('@').Last().Equals("corelogic.com", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }
        public static KeyValuePair<bool, string> LdapLogin(string userName, string password)
        {
            if (userName.IndexOf('@') != -1) userName = userName.Split('@').First();
            string adPath = string.Format("LDAP://infosolco.net");
            // string adPath = string.Format("LDAP://corelogic.com");
            LdapAuthentication auth = new LdapAuthentication(adPath);
            try
            {
                var isAuth = auth.IsAuthenticated("isc", userName, password);
                //String groups = auth.GetGroups();
                if (isAuth.Key)
                {
                    //return new KeyValuePair<bool, string>(true, groups);
                    return new KeyValuePair<bool, string>(true, isAuth.Value);
                }
                else
                {
                    return new KeyValuePair<bool, string>(false, "Authentication did not succeed. Check user name and password.");
                }
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }
        }

    }
    public class LdapAuthentication
    {
        private String _path;
        private String _filterAttribute;

        public LdapAuthentication(String path)
        {
            _path = path;
        }

        public KeyValuePair<bool, string> IsAuthenticated(String domain, String username, String pwd)
        {
            String domainAndUsername = domain + @"\" + username;
            DirectoryEntry entry = new DirectoryEntry(_path, domainAndUsername, pwd);

            try
            {//Bind to the native AdsObject to force authentication.
                Object obj = entry.NativeObject;

                DirectorySearcher search = new DirectorySearcher(entry);

                search.Filter = "(SAMAccountName=" + username + ")";
                search.PropertiesToLoad.Add("cn");
                SearchResult result = search.FindOne();

                if (null == result)
                {
                    return new KeyValuePair<bool, string>(false, string.Empty);
                }

                //Update the new path to the user in the directory.
                _path = result.Path;

                _filterAttribute = (String)result.Properties["cn"][0];
            }
            catch (Exception ex)
            {
                throw new Exception("Error authenticating user. " + ex.Message);
            }

            return new KeyValuePair<bool, string>(true, _path);
        }

    }
}