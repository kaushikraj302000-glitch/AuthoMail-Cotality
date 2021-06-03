using AM.Services;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using AM.Lib;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace AM.Web.Models
{
    public class CLAuthentication
    {
        public static bool HasCorelogicAccount(string Email)
        {
            if (Email.IsNullOrWhiteSpace()) return false;
            var mailAddress = new MailAddress(Email);
            if (mailAddress.Host.Equals("corelogic.com", StringComparison.CurrentCultureIgnoreCase)) return true;
            return false;
        }
        public static KeyValuePair<bool, string> LdapLogin(string userName, string password)
        {
            if (userName.IndexOf('@') != -1) userName = userName.Split('@').First();
            string adPath = string.Format("LDAP://infosolco.net");
            LdapAuthentication auth = new LdapAuthentication(adPath);
            return auth.IsAuthenticated("isc", userName, password);
        }

        public async Task<KeyValuePair<bool, string>> IsValidCLUser(string username)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string token = await GenerateToken();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                HttpResponseMessage httpResponse = await httpClient.GetAsync($"{Config.CLAuth_UserDetailsUrl}{username}");
                if (httpResponse.IsSuccessStatusCode)
                {
                    var response = await httpResponse.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<object>(response);
                    if (Convert.ToBoolean(user.GetValueOfPropertyFromPropertyName("enable")))
                    {
                        return new KeyValuePair<bool, string>(true, "Valid user");
                    }
                    return new KeyValuePair<bool, string>(false, "Not valid user");
                }
                else
                {
                    string error = await httpResponse.Content.ReadAsStringAsync();
                    return new KeyValuePair<bool, string>(false, error.ToString());
                }
            }

        }
        private async Task<string> GenerateToken()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{Config.CLAuth_Username}:{Config.CLAuth_Password}")));
                HttpResponseMessage tokenResponse = await httpClient.PostAsync(Config.CLAuth_TokenUrl, new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()));
                var jsonContent = await tokenResponse.Content.ReadAsStringAsync();
                var jObj = JObject.Parse(jsonContent);

                if (jObj == null || jObj.GetValue("access_token") == null)
                {
                    var msg = $"Not able to generate token.";
                    throw new Exception(msg);
                }
                return jObj.GetValue("access_token").ToString();
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
            {
                Object obj = entry.NativeObject;

                DirectorySearcher search = new DirectorySearcher(entry);

                search.Filter = "(SAMAccountName=" + username + ")";
                search.PropertiesToLoad.Add("cn");
                search.PropertiesToLoad.Add("givenName");
                search.PropertiesToLoad.Add("sn");
                search.PropertiesToLoad.Add("displayName");
                SearchResult result = search.FindOne();

                if (null == result)
                {
                    return new KeyValuePair<bool, string>(false, string.Empty);
                }

                _filterAttribute = (String)result.Properties["cn"][0];
                string userData = string.Format("{0}|{1}|{2}", (string)result.Properties["givenName"][0], (string)result.Properties["sn"][0], (string)result.Properties["displayName"][0]);
                return new KeyValuePair<bool, string>(true, userData);
            }
            catch (COMException ec)
            {
                return new KeyValuePair<bool, string>(false, ec.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Error authenticating user. " + ex.Message);
            }
        }

    }
    public class EmailValidate
    {
        public string Email { get; set; }
        public string Host { get; set; }
        public string User { get; set; }
        public bool IsCoreLogic { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public EmailValidate(string email)
        {
            try
            {
                MailAddress mailAddress = new MailAddress(email);
                Email = mailAddress.Address;
                Host = mailAddress.Host;
                User = mailAddress.User;
                IsCoreLogic = mailAddress.Host.Equals("corelogic.com", StringComparison.CurrentCultureIgnoreCase);
                IsValid = true;
            }
            catch (Exception)
            {
                IsValid = false;
                Message = "Invalid email address.";
            }
        }
    }
}