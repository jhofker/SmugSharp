using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace SmugSharp
{
    /// <summary>
    /// 
    /// Documentation: https://api.smugmug.com/api/v2
    /// </summary>
    public class SmugMug
    {
        public const string BaseApiUrl = "https://api.smugmug.com/api/v2";

        public string ApiKey { get; set; }
        public string AuthToken { get; set; }
        public Authentication Authentication { get; private set; }

        public SmugMug(string authToken, string authSecret, string apiKey, string apiSecret, string callbackUrl)
        {
            ApiKey = apiKey;
            AuthToken = authToken;
            Authentication = new Authentication(authToken, authSecret, ApiKey, apiSecret, callbackUrl);
        }

        public SmugMug(string apiKey, string apiSecret, string callbackUrl)
        {
            ApiKey = apiKey;
            Authentication = new Authentication(ApiKey, apiSecret, callbackUrl);
        }

        public async Task<string> GetResponseWithHeaders(string url)
        {
            var headers = Authentication.GetAuthHeaders(url);

            var request = new HttpClient();

            request.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "OAuth",
                    string.Join(", ", headers.OrderBy(h => h.Key).Select(h => $"{h.Key}=\"{h.Value}\"")));
            request.DefaultRequestHeaders.Add("accept", "application/json");

            var response = await request.GetAsync(url);

            string httpResponse = null;
            if (response != null)
            {
                httpResponse = await response.Content.ReadAsStringAsync();
            }
            return httpResponse;
        }
    }
}
