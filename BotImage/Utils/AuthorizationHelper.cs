using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Configuration;
using Newtonsoft.Json;

namespace BotImage.Utils
{
    public static class AuthorizationHelper
    {
        public static async Task<string> GetBearerToken()
        {
            var client = new HttpClient();

            var res = await client.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token",
                new StringContent($"client_id={ConfigurationManager.AppSettings["MicrosoftAppId"]}&client_secret={ConfigurationManager.AppSettings["MicrosoftAppPassword"]}&grant_type=client_credentials&scope=https%3A%2F%2Fgraph.microsoft.com%2F.default", Encoding.UTF8, "application/x-www-form-urlencoded"));
            try
            {
                var body = JsonConvert.DeserializeObject<AuthContext>(await res.Content.ReadAsStringAsync());
                return body.AccessToken;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.ToString());
                throw;
            }
        }
    }


    public class AuthContext
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("expires_in ")]
        public int ExpiresIn { get; set; }
        [JsonProperty("ext_expires_in")]
        public int ExtExpiresIn { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }

}