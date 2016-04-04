using System;
using System.Security.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace OAuth
{
    public class OAuthBase
    {
        /// <summary>
        /// Creates the necessary OAuth parameters for a given ApiKey and callback url.
        /// </summary>
        /// <param name="apikey">The consuming app's api key.</param>
        /// <param name="callbackUrl">The callback url to send to the api.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetOAuthParameters(string apikey, string callbackUrl = null)
        {
            var epochDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timespan = DateTime.UtcNow - epochDate;
            var oauthTimestamp = Math.Round(timespan.TotalSeconds).ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            var oauthNonce = CryptographicBuffer.GenerateRandomNumber().ToString();

            var parameters = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(callbackUrl))
            {
                parameters.Add("oauth_callback", callbackUrl);
            }
            parameters.Add("oauth_consumer_key", apikey);
            parameters.Add("oauth_nonce", oauthNonce);
            parameters.Add("oauth_signature_method", "HMAC-SHA1");
            parameters.Add("oauth_timestamp", oauthTimestamp);
            parameters.Add("oauth_version", "1.0");

            return parameters;
        }

        /// <summary>
        /// Generates a signed url for a given set of parameters, secret, destination url, and secret key.
        /// </summary>
        /// <param name="parameters">Url parameters</param>
        /// <param name="oauthTokenSecret">The OAuth token secret from the api.</param>
        /// <param name="url">The url to sign.</param>
        /// <param name="secretKey">The secret half of the api key.</param>
        /// <param name="exchangeStep">Indicates whether or not to include the token secret when generating the signing key.</param>
        /// <returns>A signed url</returns>
        public static string CalculateOAuthSignedUrl(Dictionary<string, string> parameters, string oauthTokenSecret, string url, string secretKey, bool exchangeStep)
        {
            var baseString = new StringBuilder();
            var sortedParams = new SortedDictionary<string, string>();

            foreach (var param in parameters.OrderBy(p => p.Key))
            {
                sortedParams.Add(param.Key, param.Value);
            }

            foreach (var param in sortedParams)
            {
                baseString.Append(param.Key);
                baseString.Append("=");
                baseString.Append(Uri.EscapeDataString(param.Value));
                baseString.Append("&");
            }

            //removing the extra ampersand 
            baseString.Remove(baseString.Length - 1, 1);
            var baseStringForSig = "POST&" + Uri.EscapeDataString(url) + "&" + Uri.EscapeDataString(baseString.ToString());

            //calculating the signature 
            var HmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");

            IBuffer keyMaterial;
            if (exchangeStep)
            {
                keyMaterial = CryptographicBuffer.ConvertStringToBinary(secretKey + "&" + oauthTokenSecret, BinaryStringEncoding.Utf8);
            }
            else
            {
                keyMaterial = CryptographicBuffer.ConvertStringToBinary(secretKey + "&", BinaryStringEncoding.Utf8);
            }

            var cryptoKey = HmacSha1Provider.CreateKey(keyMaterial);
            var dataString = CryptographicBuffer.ConvertStringToBinary(baseStringForSig, BinaryStringEncoding.Utf8);

            return url + "?" + baseString.ToString() + "&oauth_signature=" +
                Uri.EscapeDataString(CryptographicBuffer.EncodeToBase64String(CryptographicEngine.Sign(cryptoKey, dataString)));
        }

        /// <summary>
        /// Generates a HMAC-SHA1 signature for a given set of parameters
        /// </summary>
        /// <param name="method">The HTTP method that will be used. (GET, POST, PATCH, PUT, DELETE)</param>
        /// <param name="url">The destination url.</param>
        /// <param name="data">Data that will be included in the request</param>
        /// <param name="apiSecret">The Api secret. Used for signing.</param>
        /// <param name="secretKey">The access token secret. Used for signing.</param>
        /// <returns></returns>
        public static string GetSignature(string method, string url, string data, string apiSecret, string secretKey)
        {
            var HmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);

            var keyMaterial = CryptographicBuffer.ConvertStringToBinary(apiSecret + "&" + secretKey, BinaryStringEncoding.Utf8);
            var cryptoKey = HmacSha1Provider.CreateKey(keyMaterial);

            var baseStringForSig = $"{method}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(data)}";
            var dataString = CryptographicBuffer.ConvertStringToBinary(baseStringForSig, BinaryStringEncoding.Utf8);

            return Uri.EscapeDataString(CryptographicBuffer.EncodeToBase64String(CryptographicEngine.Sign(cryptoKey, dataString)));
        }

        /// <summary>
        /// Does a POST to a url and returns the response.
        /// </summary>
        /// <param name="url">The destination url.</param>
        /// <returns>The response from the endpoint.</returns>
        /// <remarks>Will likely go away in future releases.</remarks>
        public async static Task<string> GetResponseFromWeb(string url)
        {
            var Request = (HttpWebRequest)WebRequest.Create(url);
            Request.Method = "POST";

            var response = (HttpWebResponse)await Request.GetResponseAsync();

            string httpResponse = null;
            if (response != null)
            {
                var data = new StreamReader(response.GetResponseStream());
                httpResponse = await data.ReadToEndAsync();
            }
            return httpResponse;
        }
    }

}