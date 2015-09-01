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
        /// <summary>
        /// The base url of the SmugMug v2 api.
        /// </summary>
        public const string BaseApiUrl = "https://api.smugmug.com/api/v2";

        /// <summary>
        /// The consuming application's api key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// The authtoken for the current user.
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// An instance of the <see cref="Authentication"/> class used to control and contain
        /// authentication information for the current app and user.
        /// </summary>
        public Authentication Authentication { get; private set; }

        /// <summary>
        /// The ctor to use in testing or situations where you already have the auth token and secret.
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="authSecret"></param>
        /// <param name="apiKey"></param>
        /// <param name="apiSecret"></param>
        /// <param name="callbackUrl"></param>
        public SmugMug(string authToken, string authSecret, string apiKey, string apiSecret, string callbackUrl)
        {
            ApiKey = apiKey;
            AuthToken = authToken;
            Authentication = new Authentication(authToken, authSecret, ApiKey, apiSecret, callbackUrl);
        }

        /// <summary>
        /// The ctor to use when the user will need to authenticate.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="apiSecret"></param>
        /// <param name="callbackUrl"></param>
        public SmugMug(string apiKey, string apiSecret, string callbackUrl)
        {
            ApiKey = apiKey;
            Authentication = new Authentication(ApiKey, apiSecret, callbackUrl);
        }

        /// <summary>
        /// Performs and authenticated request to a given url.
        /// </summary>
        /// <param name="url">The destination url</param>
        /// <returns>The response from the request</returns>
        /// <remarks>Will likely go away in the future.</remarks>
        public async Task<string> GetResponseWithHeaders(string url)
        {
            var headers = Authentication.GetAuthHeaders("GET", url);

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
